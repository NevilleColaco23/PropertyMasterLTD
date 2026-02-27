using System.Text;
using System.Text.Json;
using Messaging.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Domain.AccessLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AccessLogWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly Messaging.Shared.RabbitMqOptions _options;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private IConnection? _connection;
        private IChannel? _channel;

        public Worker(ILogger<Worker> logger, IOptions<Messaging.Shared.RabbitMqOptions> options, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _options = options.Value;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting AccessLogWorker - connecting to RabbitMQ at {Host}:{Port}", _options.Host, _options.Port);

                var factory = new ConnectionFactory
                {
                    HostName = _options.Host,
                    Port = _options.Port,
                    UserName = _options.Username,
                    Password = _options.Password,
                    VirtualHost = _options.VirtualHost
                };

                _connection = await factory.CreateConnectionAsync(cancellationToken);
                _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
                await _channel.ExchangeDeclareAsync(_options.Exchange, ExchangeType.Direct, durable: true, cancellationToken: cancellationToken);
                await _channel.QueueDeclareAsync(_options.Queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
                await _channel.QueueBindAsync(_options.Queue, _options.Exchange, _options.RoutingKey, cancellationToken: cancellationToken);
                await base.StartAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start AccessLogWorker. Make sure RabbitMQ is running at {Host}:{Port} with credentials User={Username}",
                    _options.Host, _options.Port, _options.Username);
                throw;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_channel == null) throw new InvalidOperationException("RabbitMQ channel not initialized");

            try
            {
                _logger.LogInformation("Setting up RabbitMQ consumer for queue: {Queue}", _options.Queue);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (_, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var json = Encoding.UTF8.GetString(body);

                        _logger.LogInformation("=== MESSAGE RECEIVED ===");
                        _logger.LogInformation("Raw JSON: {Json}", json);

                        var logEvent = JsonSerializer.Deserialize<Messaging.Shared.Models.AccessLogEvent>(json);
                        _logger.LogInformation("Deserialized log event: {@LogEvent}", logEvent);

                        if (logEvent != null)
                        {
                            _logger.LogInformation("Creating AccessLog entry for event: {Path}", logEvent.Path);

                            // Parse UserId from string to int
                            int.TryParse(logEvent.UserId, out int userId);

                            // Create AccessLog domain entity (same as CreateLogCommand)
                            var logEntry = new AccessLog(
                                id: 0, // Repository.Add() will auto-generate using CounterService
                                log: logEvent.Path ?? "",
                                user: userId,
                                time: logEvent.TimestampUtc,
                                action: logEvent.Method ?? "N/A",
                                details: $"Status: {logEvent.StatusCode}, Duration: {logEvent.DurationMs}ms, TraceId: {logEvent.TraceId}, IP: {logEvent.ClientIp}, Agent: {logEvent.UserAgent}"
                            );

                            // Create a scope to resolve scoped services (same pattern as BackgroundService processing)
                            using (var scope = _serviceScopeFactory.CreateScope())
                            {
                                var accessLogRepository = scope.ServiceProvider.GetRequiredService<IAccessLogRepository>();

                                // Use repository.Add() - same as CreateLogCommand via UnitOfWork
                                await accessLogRepository.Add(logEntry, stoppingToken);

                                _logger.LogInformation("✅ Successfully saved log event to MongoDB with ID: {Id}", logEntry.Id);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Deserialized log event is null!");
                        }

                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        _logger.LogInformation("✅ Message acknowledged successfully");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error processing message, rejecting and requeueing");
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true);
                    }
                };

                await _channel.BasicConsumeAsync(_options.Queue, false, consumer, cancellationToken: stoppingToken);
                _logger.LogInformation("Started consuming messages from queue: {Queue}", _options.Queue);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Worker is stopping");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExecuteAsync");
                throw;
            }
        }

        public override void Dispose()
        {
            if (_channel is not null)
            {
                try { _channel.CloseAsync().GetAwaiter().GetResult(); } catch { }
            }
            if (_connection is not null)
            {
                try { _connection.CloseAsync().GetAwaiter().GetResult(); } catch { }
            }
            base.Dispose();
        }
    }
}