using MongoDB.Driver;
using Microsoft.Extensions.Logging;

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

        public EmailQueueService(IMongoDatabase database, ILogger<EmailQueueService> logger)
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
                Status = EmailOutboxStatus.Pending,
                NextRunAtUtc = DateTime.UtcNow,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _collection.InsertOneAsync(message);
            _logger.LogInformation("Queued email {Id} to {To} with subject '{Subject}'", message.Id, to, subject);
        }
    }

    public enum EmailOutboxStatus
    {
        Pending = 0,
        Processing = 1,
        Sent = 2,
        Failed = 3
    }

    public class EmailOutboxMessage
    {
        public int Id { get; set; }
        public string Type { get; set; } = "activation";
        public string To { get; set; } = default!;
        public string Subject { get; set; } = default!;
        public string BodyHtml { get; set; } = default!;
        public EmailOutboxStatus Status { get; set; } = EmailOutboxStatus.Pending;
        public int Attempts { get; set; } = 0;
        public DateTime NextRunAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? LockedUntilUtc { get; set; }
        public string? LastError { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? SentAtUtc { get; set; }
    }
}
