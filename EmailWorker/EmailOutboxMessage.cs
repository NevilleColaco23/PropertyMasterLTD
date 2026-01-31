using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace EmailWorker
{
    public enum EmailOutboxStatus
    {
        Pending = 0,
        Processing = 1,
        Sent = 2,
        Failed = 3
    }

    public class EmailOutboxMessage
    {
        [BsonId]
        public int Id { get; set; }

        [BsonElement("type")]
        public string Type { get; set; } = "activation";

        [BsonElement("to")]
        public string To { get; set; } = default!;

        [BsonElement("subject")]
        public string Subject { get; set; } = default!;

        [BsonElement("bodyHtml")]
        public string BodyHtml { get; set; } = default!;

        [BsonElement("status")]
        public EmailOutboxStatus Status { get; set; } = EmailOutboxStatus.Pending;

        [BsonElement("attempts")]
        public int Attempts { get; set; } = 0;

        [BsonElement("nextRunAtUtc")]
        public DateTime NextRunAtUtc { get; set; } = DateTime.UtcNow;

        [BsonElement("lockedUntilUtc")]
        [BsonIgnoreIfNull]
        public DateTime? LockedUntilUtc { get; set; }

        [BsonElement("lastError")]
        [BsonIgnoreIfNull]
        public string? LastError { get; set; }

        [BsonElement("createdAtUtc")]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [BsonElement("sentAtUtc")]
        [BsonIgnoreIfNull]
        public DateTime? SentAtUtc { get; set; }
        public int UserId { get; set; }
    }

    public static class EmailOutboxCollection
    {
        public const string Name = "EmailOutbox";
    }
}
