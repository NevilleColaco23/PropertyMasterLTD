using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using Messaging.Shared;
using Messaging.Shared.Models;
using MongoDB.Bson;

namespace MyWarehouse.Infrastructure.Services
{
    public interface IEmailQueueService
    {
        Task QueueEmailAsync(string to, string subject, string bodyHtml, string type = "activation");
    }

    public class EmailQueueService : IEmailQueueService
    {
        private readonly IMongoCollection<EmailOutboxMessage> _collection;
        private readonly IMongoCollection<BsonDocument> _keyCounters;
        private readonly IServiceBusPublisher _serviceBusPublisher;
        private readonly ILogger<EmailQueueService> _logger;
        private const string EmailOutboxCounterKey = "EmailOutbox";

        public EmailQueueService(
            IMongoDatabase database,
            IServiceBusPublisher serviceBusPublisher,
            ILogger<EmailQueueService> logger)
        {
            _collection     = database.GetCollection<EmailOutboxMessage>("EmailOutbox");
            _keyCounters    = database.GetCollection<BsonDocument>("KeyCounter");
            _serviceBusPublisher = serviceBusPublisher;
            _logger         = logger;
        }

        public async Task QueueEmailAsync(string to, string subject, string bodyHtml, string type = "activation")
        {
            // Atomically increment the counter — safe under concurrent requests
            var filter = Builders<BsonDocument>.Filter.Eq("_id", EmailOutboxCounterKey);
            var update = Builders<BsonDocument>.Update.Inc("seq", 1);
            var options = new FindOneAndUpdateOptions<BsonDocument>
            {
                IsUpsert    = true,
                ReturnDocument = ReturnDocument.After
            };

            var counterDoc = await _keyCounters.FindOneAndUpdateAsync(filter, update, options);
            var nextId = counterDoc["seq"].AsInt32;

            var message = new EmailOutboxMessage
            {
                Id           = nextId,
                Type         = type,
                To           = to,
                Subject      = subject,
                BodyHtml     = bodyHtml,
                Status       = EmailOutboxStatus.Queued,
                NextRunAtUtc = DateTime.UtcNow,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _collection.InsertOneAsync(message);
            _logger.LogInformation("Queued email {Id} to MongoDB outbox: To={To}, Subject='{Subject}'",
                message.Id, to, subject);

            // Publish to Azure Service Bus so EmailWorker picks it up immediately (no polling)
            try
            {
                var emailEvent = new EmailQueuedEvent
                {
                    EmailId      = message.Id,
                    Type         = message.Type,
                    To           = message.To,
                    Subject      = message.Subject,
                    BodyHtml     = message.BodyHtml,
                    QueuedAtUtc  = message.CreatedAtUtc
                };

                await _serviceBusPublisher.SendAsync(emailEvent, ServiceBusQueueNames.Email);
                _logger.LogInformation("Published EmailQueuedEvent {Id} to Service Bus", message.Id);
            }
            catch (Exception ex)
            {
                // Service Bus publish failure is non-fatal — the EmailWorker can still
                // pick it up on its fallback polling pass if needed.
                _logger.LogError(ex, "Failed to publish EmailQueuedEvent {Id} to Service Bus", message.Id);
            }
        }
    }

    public enum EmailOutboxStatus
    {
        Pending = 0,
        Queued = 1,
        Processing = 2,
        Sent = 3,
        Failed = 4
    }

    public class EmailOutboxMessage
    {
        [MongoDB.Bson.Serialization.Attributes.BsonId]
        public int Id { get; set; }

        [MongoDB.Bson.Serialization.Attributes.BsonElement("type")]
        public string Type { get; set; } = "activation";

        [MongoDB.Bson.Serialization.Attributes.BsonElement("to")]
        public string To { get; set; } = default!;

        [MongoDB.Bson.Serialization.Attributes.BsonElement("subject")]
        public string Subject { get; set; } = default!;

        [MongoDB.Bson.Serialization.Attributes.BsonElement("bodyHtml")]
        public string BodyHtml { get; set; } = default!;

        [MongoDB.Bson.Serialization.Attributes.BsonElement("status")]
        public EmailOutboxStatus Status { get; set; } = EmailOutboxStatus.Pending;

        [MongoDB.Bson.Serialization.Attributes.BsonElement("attempts")]
        public int Attempts { get; set; } = 0;

        [MongoDB.Bson.Serialization.Attributes.BsonElement("nextRunAtUtc")]
        public DateTime NextRunAtUtc { get; set; } = DateTime.UtcNow;

        [MongoDB.Bson.Serialization.Attributes.BsonElement("lockedUntilUtc")]
        [MongoDB.Bson.Serialization.Attributes.BsonIgnoreIfNull]
        public DateTime? LockedUntilUtc { get; set; }

        [MongoDB.Bson.Serialization.Attributes.BsonElement("lastError")]
        [MongoDB.Bson.Serialization.Attributes.BsonIgnoreIfNull]
        public string? LastError { get; set; }

        [MongoDB.Bson.Serialization.Attributes.BsonElement("createdAtUtc")]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [MongoDB.Bson.Serialization.Attributes.BsonElement("sentAtUtc")]
        [MongoDB.Bson.Serialization.Attributes.BsonIgnoreIfNull]
        public DateTime? SentAtUtc { get; set; }
    }
}
