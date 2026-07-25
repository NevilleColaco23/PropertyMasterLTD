using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using Messaging.Shared;
using Messaging.Shared.Models;

namespace MyWarehouse.Infrastructure.Services
{
    public interface IEmailQueueService
    {
        Task QueueEmailAsync(string to, string subject, string bodyHtml, string type = "activation");
    }

    public class EmailQueueService : IEmailQueueService
    {
        private readonly IMongoCollection<EmailOutboxMessage> _collection;
        private readonly ILogger<EmailQueueService> _logger;

        public EmailQueueService(
            IMongoDatabase database,
            ILogger<EmailQueueService> logger)
        {
            _collection = database.GetCollection<EmailOutboxMessage>("EmailOutbox");
            _logger = logger;
        }

        public async Task QueueEmailAsync(string to, string subject, string bodyHtml, string type = "activation")
        {
            var maxId = await _collection
                .Find(FilterDefinition<EmailOutboxMessage>.Empty)
                .SortByDescending(x => x.Id)
                .Limit(1)
                .Project(x => x.Id)
                .FirstOrDefaultAsync();

            var message = new EmailOutboxMessage
            {
                Id = maxId + 1,
                Type = type,
                To = to,
                Subject = subject,
                BodyHtml = bodyHtml,
                Status = EmailOutboxStatus.Queued,
                NextRunAtUtc = DateTime.UtcNow,
                CreatedAtUtc = DateTime.UtcNow
            };

            // Persist to MongoDB — EmailWorker polls this collection and sends the email
            await _collection.InsertOneAsync(message);
            _logger.LogInformation("Queued email {Id} to MongoDB outbox: To={To}, Subject='{Subject}'",
                message.Id, to, subject);
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
