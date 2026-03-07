using MongoDB.Driver;
using System.Text;
using System.Text.Json;
using Messaging.Shared;
using Messaging.Shared.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EmailWorker
{
    public class Worker : BackgroundService
    {
        private readonly IMongoCollection<EmailOutboxMessage> _collection;
        private readonly IEmailSender _sender;
        private readonly ILogger<Worker> _logger;
        private readonly RabbitMqOptions? _rabbitMqOptions;
        private IConnection? _connection;
        private IChannel? _channel;
        private bool _useRabbitMq = false;

        private const int MaxAttempts = 5;

        public Worker(
            IMongoDatabase db, 
            IEmailSender sender, 
            ILogger<Worker> logger,
            IOptions<RabbitMqOptions>? rabbitMqOptions = null)
        {
            _collection = db.GetCollection<EmailOutboxMessage>(EmailOutboxCollection.Name);
            _sender = sender;
            _logger = logger;
            _rabbitMqOptions = rabbitMqOptions?.Value;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("EmailWorker starting...");

            // Try to connect to RabbitMQ if configured
            if (_rabbitMqOptions != null)
            {
                try
                {
                    _logger.LogInformation("Attempting to connect to RabbitMQ at {Host}:{Port}", 
                        _rabbitMqOptions.Host, _rabbitMqOptions.Port);

                    var factory = new ConnectionFactory
                    {
                        HostName = _rabbitMqOptions.Host,
                        Port = _rabbitMqOptions.Port,
                        UserName = _rabbitMqOptions.Username,
                        Password = _rabbitMqOptions.Password,
                        VirtualHost = _rabbitMqOptions.VirtualHost
                    };

                    if (_rabbitMqOptions.UseSsl)
                    {
                        factory.Ssl = new RabbitMQ.Client.SslOption
                        {
                            Enabled = true,
                            ServerName = _rabbitMqOptions.Host,
                            AcceptablePolicyErrors = System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch |
                                                      System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors
                        };
                    }

                    _connection = await factory.CreateConnectionAsync(cancellationToken);
                    _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

                    await _channel.ExchangeDeclareAsync("email.exchange", ExchangeType.Direct, durable: true, cancellationToken: cancellationToken);
                    await _channel.QueueDeclareAsync("email.queue", durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
                    await _channel.QueueBindAsync("email.queue", "email.exchange", "email", cancellationToken: cancellationToken);

                    _useRabbitMq = true;
                    _logger.LogInformation("✅ RabbitMQ connected successfully. Using event-driven email processing.");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️  Failed to connect to RabbitMQ. Falling back to MongoDB polling.");
                    _useRabbitMq = false;
                }
            }
            else
            {
                _logger.LogInformation("ℹ️  RabbitMQ not configured. Using MongoDB polling.");
            }

            try
            {
                var count = await _collection.CountDocumentsAsync(FilterDefinition<EmailOutboxMessage>.Empty, cancellationToken: cancellationToken);
                _logger.LogInformation("MongoDB connected. EmailOutbox collection has {Count} documents.", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to MongoDB. Check connection string.");
            }

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EmailWorker started.");

            if (_useRabbitMq && _channel != null)
            {
                await ExecuteRabbitMqMode(stoppingToken);
            }
            else
            {
                await ExecutePollingMode(stoppingToken);
            }
        }

        private async Task ExecuteRabbitMqMode(CancellationToken stoppingToken)
        {
            if (_channel == null) throw new InvalidOperationException("RabbitMQ channel not initialized");

            _logger.LogInformation("🐰 Starting RabbitMQ consumer for email.queue");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var emailEvent = JsonSerializer.Deserialize<EmailQueuedEvent>(json);

                    if (emailEvent != null)
                    {
                        _logger.LogInformation("📧 Received email event: EmailId={EmailId}, To={To}", 
                            emailEvent.EmailId, emailEvent.To);

                        var msg = await _collection.Find(x => x.Id == emailEvent.EmailId).FirstOrDefaultAsync(stoppingToken);

                        if (msg != null && msg.Status != EmailOutboxStatus.Sent)
                        {
                            await _collection.UpdateOneAsync(
                                x => x.Id == msg.Id,
                                Builders<EmailOutboxMessage>.Update.Set(x => x.Status, EmailOutboxStatus.Processing),
                                cancellationToken: stoppingToken);

                            await ProcessOneAsync(msg, stoppingToken);
                            await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                        }
                        else
                        {
                            _logger.LogWarning("Email {EmailId} not found or already sent.", emailEvent.EmailId);
                            await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing RabbitMQ message");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
                }
            };

            await _channel.BasicConsumeAsync("email.queue", false, consumer, stoppingToken);
            _logger.LogInformation("✅ RabbitMQ consumer started. Waiting for email events...");

            _ = Task.Run(async () => await ExecuteFallbackPolling(stoppingToken), stoppingToken);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task ExecutePollingMode(CancellationToken stoppingToken)
        {
            _logger.LogInformation("📊 Starting MongoDB polling mode (every 2 seconds)");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var msg = await ClaimOneAsync(stoppingToken);
                    if (msg is null)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                        continue;
                    }

                    await ProcessOneAsync(msg, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Polling loop error");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private async Task ExecuteFallbackPolling(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🔄 Starting fallback polling (every 30 seconds)");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

                    var missedEmails = await _collection.Find(x => 
                        (x.Status == EmailOutboxStatus.Queued || x.Status == EmailOutboxStatus.Pending) &&
                        x.NextRunAtUtc <= DateTime.UtcNow)
                        .Limit(5)
                        .ToListAsync(stoppingToken);

                    if (missedEmails.Any())
                    {
                        _logger.LogWarning("⚠️  Found {Count} missed emails. Processing via fallback.", missedEmails.Count);

                        foreach (var msg in missedEmails)
                        {
                            try
                            {
                                await _collection.UpdateOneAsync(
                                    x => x.Id == msg.Id && x.Status != EmailOutboxStatus.Sent,
                                    Builders<EmailOutboxMessage>.Update.Set(x => x.Status, EmailOutboxStatus.Processing),
                                    cancellationToken: stoppingToken);

                                await ProcessOneAsync(msg, stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Fallback processing error for email {EmailId}", msg.Id);
                            }
                        }
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fallback polling error");
                }
            }
        }

        private Task<EmailOutboxMessage?> ClaimOneAsync(CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var lockUntil = now.AddMinutes(2);

            // Claim:
            // 1) Pending and due to run
            // 2) OR previously claimed (Processing) but lock expired (worker crashed / timeout)
            var filter = Builders<EmailOutboxMessage>.Filter.Or(
                Builders<EmailOutboxMessage>.Filter.And(
                    Builders<EmailOutboxMessage>.Filter.Eq(x => x.Status, EmailOutboxStatus.Pending),
                    Builders<EmailOutboxMessage>.Filter.Lte(x => x.NextRunAtUtc, now)
                ),
                Builders<EmailOutboxMessage>.Filter.And(
                    Builders<EmailOutboxMessage>.Filter.Eq(x => x.Status, EmailOutboxStatus.Processing),
                    Builders<EmailOutboxMessage>.Filter.Lte(x => x.LockedUntilUtc, now)
                )
            );

            var update = Builders<EmailOutboxMessage>.Update
                .Set(x => x.Status, EmailOutboxStatus.Processing)
                .Set(x => x.LockedUntilUtc, lockUntil)
                .Inc(x => x.Attempts, 1);

            return _collection.FindOneAndUpdateAsync(
                filter,
                update,
                new FindOneAndUpdateOptions<EmailOutboxMessage>
                {
                    ReturnDocument = ReturnDocument.After,
                    Sort = Builders<EmailOutboxMessage>.Sort.Ascending(x => x.CreatedAtUtc)
                },
                ct
            );
        }

        private async Task ProcessOneAsync(EmailOutboxMessage msg, CancellationToken ct)
        {
            try
            {
                await _sender.SendHtmlAsync(msg.To, msg.Subject, msg.BodyHtml, ct);

                var update = Builders<EmailOutboxMessage>.Update
                    .Set(x => x.Status, EmailOutboxStatus.Sent)
                    .Set(x => x.SentAtUtc, DateTime.UtcNow)
                    .Set(x => x.LockedUntilUtc, null)
                    .Set(x => x.LastError, null);

                await _collection.UpdateOneAsync(x => x.Id == msg.Id, update, cancellationToken: ct);
                _logger.LogInformation("Sent outbox email {Id} -> {To}", msg.Id, msg.To);
            }
            catch (Exception ex)
            {
                var attempts = msg.Attempts;
                var terminal = attempts >= MaxAttempts;

                var delayMinutes = Math.Min(Math.Pow(2, attempts), 60);
                var nextRun = DateTime.UtcNow.AddMinutes(delayMinutes);

                var update = Builders<EmailOutboxMessage>.Update
                    .Set(x => x.Status, terminal ? EmailOutboxStatus.Failed : EmailOutboxStatus.Pending)
                    .Set(x => x.LockedUntilUtc, null)
                    .Set(x => x.NextRunAtUtc, nextRun)
                    .Set(x => x.LastError, ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message);

                await _collection.UpdateOneAsync(x => x.Id == msg.Id, update, cancellationToken: ct);

                _logger.LogError(ex, "Failed outbox email {Id} attempt {Attempt}/{Max}", msg.Id, attempts, MaxAttempts);
            }
        }
    }
}