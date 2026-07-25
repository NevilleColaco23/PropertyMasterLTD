namespace Messaging.Shared.Models
{
    /// <summary>
    /// Message contract published to Azure Service Bus when a user activity occurs.
    /// Consumed by AccessLogWorker and saved to MongoDB as a UserActivityLog.
    /// </summary>
    public class UserActivityEvent
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Numeric value of the ActivityType enum.
        /// Stored as int to avoid enum serialization issues across service boundaries.
        /// Maps to MyWarehouse.Domain.UserActivity.ActivityType.
        /// </summary>
        public int ActivityType { get; set; }

        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? DisplayMessage { get; set; }
        public string? IPAddress { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, string>? Metadata { get; set; }
    }
}
