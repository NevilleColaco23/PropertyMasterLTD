using MongoDB.Bson.Serialization.Attributes;
using MyWarehouse.Domain.Common;

namespace MyWarehouse.Domain.System_Related.EmailOutbox
{
    public enum EmailOutboxStatus
    {
        Pending = 0,
        Processing = 1,
        Sent = 2,
        Failed = 3
    }

    public class EmailOutbox : IEntity<int>
    {
        public int Id { get; set; }

        [BsonElement("type")]
        public string Type { get; set; } = "activation";

        [BsonElement("to")]
        public string To { get; set; } = default!;

        [BsonElement("subject")]
        public string Subject { get; set; } = default!;

        [BsonElement("bodyHtml")]
        public string BodyHtml { get; set; } = default!;

        // Stored as integer in Mongo by default (0,1,2,3) matching your sample
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

        // For Mongo deserialization
        private EmailOutbox() { }

        public EmailOutbox(int id, string to, string subject, string bodyHtml, string type = "activation", int userId = 0)
        {
            //Id = id;
            Type = type;
            To = to;
            Subject = subject;
            BodyHtml = bodyHtml;

            Status = EmailOutboxStatus.Pending;
            Attempts = 0;
            NextRunAtUtc = DateTime.UtcNow;
            LockedUntilUtc = null;
            LastError = null;
            CreatedAtUtc = DateTime.UtcNow;
            SentAtUtc = null;
            UserId = userId;
        }
    }
}
