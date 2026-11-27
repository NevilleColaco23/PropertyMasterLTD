using MongoDB.Driver;
using MyWarehouse.Infrastructure.Services;

namespace MyWarehouse.WebApi.Email
{
    /// <summary>
    /// Background service inside the API that sends rows from the MongoDB "EmailOutbox" collection
    /// (replaces the Service Bus + EmailWorker path). It runs when signalled by EmailQueueService and
    /// every <see cref="PollInterval"/> as a safety net. Failed sends are retried with exponential
    /// backoff up to <see cref="MaxAttempts"/>, then marked Failed.
    /// </summary>
    public class EmailOutboxProcessor : BackgroundService
    {
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(2);
        private const int MaxAttempts = 5;
        private const int MaxPerCycle = 25;

        private readonly IServiceScopeFactory _scopes;
        private readonly IEmailOutboxSignal _signal;
        private readonly IConfiguration _config;
        private readonly ILogger<EmailOutboxProcessor> _logger;
        private bool _warnedMissingKey;

        public EmailOutboxProcessor(
            IServiceScopeFactory scopes,
            IEmailOutboxSignal signal,
            IConfiguration config,
            ILogger<EmailOutboxProcessor> logger)
        {
            _scopes = scopes;
            _signal = signal;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Email outbox processing pass failed");
                }

                try
                {
                    await _signal.WaitAsync(PollInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task ProcessPendingAsync(CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(_config["Resend:ApiKey"]))
            {
                // Leave rows queued; they are sent once a key is configured.
                if (!_warnedMissingKey)
                {
                    _logger.LogWarning("Resend:ApiKey is not configured; queued emails will not be sent.");
                    _warnedMissingKey = true;
                }
                return;
            }
            _warnedMissingKey = false;

            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
            var sender = scope.ServiceProvider.GetRequiredService<IOutboxEmailSender>();
            var collection = db.GetCollection<EmailOutboxMessage>("EmailOutbox");

            for (var i = 0; i < MaxPerCycle && !ct.IsCancellationRequested; i++)
            {
                var message = await ClaimNextAsync(collection, ct);
                if (message == null)
                {
                    return;
                }

                await SendAndRecordAsync(collection, sender, message, ct);
            }
        }

        /// <summary>Atomically claims one due row so two API instances never send the same email.</summary>
        private static async Task<EmailOutboxMessage?> ClaimNextAsync(
            IMongoCollection<EmailOutboxMessage> collection, CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var f = Builders<EmailOutboxMessage>.Filter;

            var due = f.And(
                f.In(m => m.Status, new[] { EmailOutboxStatus.Pending, EmailOutboxStatus.Queued }),
                f.Lte(m => m.NextRunAtUtc, now),
                f.Lt(m => m.Attempts, MaxAttempts));

            // Recover rows left in Processing by a crashed instance once their lock expires.
            var stuck = f.And(
                f.Eq(m => m.Status, EmailOutboxStatus.Processing),
                f.Lt(m => m.LockedUntilUtc, (DateTime?)now),
                f.Lt(m => m.Attempts, MaxAttempts));

            var update = Builders<EmailOutboxMessage>.Update
                .Set(m => m.Status, EmailOutboxStatus.Processing)
                .Set(m => m.LockedUntilUtc, now + LockDuration)
                .Inc(m => m.Attempts, 1);

            return await collection.FindOneAndUpdateAsync(
                f.Or(due, stuck),
                update,
                new FindOneAndUpdateOptions<EmailOutboxMessage> { ReturnDocument = ReturnDocument.After },
                ct);
        }

        private async Task SendAndRecordAsync(
            IMongoCollection<EmailOutboxMessage> collection,
            IOutboxEmailSender sender,
            EmailOutboxMessage message,
            CancellationToken ct)
        {
            var u = Builders<EmailOutboxMessage>.Update;
            try
            {
                await sender.SendHtmlAsync(message.To, message.Subject, message.BodyHtml, ct);

                await collection.UpdateOneAsync(
                    m => m.Id == message.Id,
                    u.Set(m => m.Status, EmailOutboxStatus.Sent)
                     .Set(m => m.SentAtUtc, DateTime.UtcNow)
                     .Unset(m => m.LockedUntilUtc)
                     .Unset(m => m.LastError),
                    cancellationToken: CancellationToken.None);
            }
            catch (Exception ex)
            {
                var exhausted = message.Attempts >= MaxAttempts;
                var backoff = TimeSpan.FromSeconds(30 * Math.Pow(2, message.Attempts - 1));

                _logger.LogError(ex, "Sending email {Id} failed (attempt {Attempt}/{Max})",
                    message.Id, message.Attempts, MaxAttempts);

                await collection.UpdateOneAsync(
                    m => m.Id == message.Id,
                    u.Set(m => m.Status, exhausted ? EmailOutboxStatus.Failed : EmailOutboxStatus.Queued)
                     .Set(m => m.NextRunAtUtc, DateTime.UtcNow + backoff)
                     .Set(m => m.LastError, ex.Message)
                     .Unset(m => m.LockedUntilUtc),
                    cancellationToken: CancellationToken.None);
            }
        }
    }
}
