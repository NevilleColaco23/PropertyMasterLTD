using MongoDB.Driver;

namespace EmailWorker
{
    public class Worker : BackgroundService
    {
        private readonly IMongoCollection<EmailOutboxMessage> _collection;
        private readonly IEmailSender _sender;
        private readonly ILogger<Worker> _logger;

        private const int MaxAttempts = 5;

        public Worker(IMongoDatabase db, IEmailSender sender, ILogger<Worker> logger)
        {
            _collection = db.GetCollection<EmailOutboxMessage>(EmailOutboxCollection.Name);
            _sender = sender;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EmailWorker started.");

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
                    _logger.LogError(ex, "Worker loop error.");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
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