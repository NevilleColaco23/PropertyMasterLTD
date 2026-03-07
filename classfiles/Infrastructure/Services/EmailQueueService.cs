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
        private readonly IRabbitMqPublisher _rabbitMqPublisher;
        private readonly ILogger<EmailQueueService> _logger;

        public EmailQueueService(
            IMongoDatabase database, 
            IRabbitMqPublisher rabbitMqPublisher,
            ILogger<EmailQueueService> logger)
        {
            _collection = database.GetCollection<EmailOutboxMessage>("EmailOutbox");
            _rabbitMqPublisher = rabbitMqPublisher;
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
                Status = EmailOutboxStatus.Queued, // Changed from Pending to Queued
                NextRunAtUtc = DateTime.UtcNow,
                CreatedAtUtc = DateTime.UtcNow
            };

            // Step 1: Persist to MongoDB for audit trail
            await _collection.InsertOneAsync(message);
            _logger.LogInformation("Persisted email {Id} to MongoDB for audit: To={To}, Subject='{Subject}'", 
                message.Id, to, subject);

            // Step 2: Publish to RabbitMQ for real-time processing
            try
            {
                var emailEvent = new EmailQueuedEvent
                {
                    EmailId = message.Id,
                    Type = message.Type,
                    To = message.To,
                    Subject = message.Subject,
                    BodyHtml = message.BodyHtml,
                    QueuedAtUtc = message.CreatedAtUtc
                };

                _rabbitMqPublisher.Publish(emailEvent, "email.exchange", "email");

                _logger.LogInformation("Published email {Id} to RabbitMQ for processing: To={To}", 
                    message.Id, to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish email {Id} to RabbitMQ. Email will retry via fallback polling.", 
                    message.Id);
                // Email is still in MongoDB, EmailWorker can fall back to polling if RabbitMQ fails
            }
        }
    }

    public enum EmailOutboxStatus
    {
        Pending = 0,        // Waiting for processing (fallback polling)
        Queued = 1,         // Published to RabbitMQ, awaiting consumption
        Processing = 2,     // Currently being processed
        Sent = 3,           // Successfully sent
        Failed = 4          // Failed after max attempts
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
