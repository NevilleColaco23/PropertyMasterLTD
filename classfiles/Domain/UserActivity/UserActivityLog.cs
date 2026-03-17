using MongoDB.Bson.Serialization.Attributes;
using MyWarehouse.Domain.Common;
using System;
using System.Collections.Generic;

namespace MyWarehouse.Domain.UserActivity
{
    /// <summary>
    /// Entity representing a user activity log entry
    /// Tracks all user actions across the system for monitoring and audit purposes
    /// </summary>
    public class UserActivityLog : IEntity<int>
    {
        [BsonId]
        public int Id { get; set; }

        /// <summary>
        /// User ID who performed the activity
        /// </summary>
        [BsonElement("UserId")]
        public int UserId { get; set; }

        /// <summary>
        /// Username for easier querying and display
        /// </summary>
        [BsonElement("Username")]
        public string Username { get; set; }

        /// <summary>
        /// Type of activity performed (Create, Update, Delete, etc.)
        /// </summary>
        [BsonElement("ActivityType")]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public ActivityType ActivityType { get; set; }

        /// <summary>
        /// Type of entity affected (Property, Room, Booking, etc.)
        /// Null if activity is not entity-specific
        /// </summary>
        [BsonElement("EntityType")]
        public string? EntityType { get; set; }

        /// <summary>
        /// ID of the affected entity
        /// Null if activity is not entity-specific
        /// </summary>
        [BsonElement("EntityId")]
        public int? EntityId { get; set; }

        /// <summary>
        /// Short action description (e.g., "Created property", "Logged in")
        /// </summary>
        [BsonElement("Action")]
        public string Action { get; set; }

        /// <summary>
        /// Detailed description of the activity
        /// </summary>
        [BsonElement("Description")]
        public string Description { get; set; }

        /// <summary>
        /// Additional metadata as JSON (flexible for different activity types)
        /// </summary>
        [BsonElement("Metadata")]
        public Dictionary<string, object>? Metadata { get; set; }

        /// <summary>
        /// Human-readable activity message for reports and widgets
        /// Example: "John Doe viewed All Dashboards while working on Sunset Villa property"
        /// </summary>
        [BsonElement("DisplayMessage")]
        public string? DisplayMessage { get; set; }

        /// <summary>
        /// Timestamp when the activity occurred (UTC)
        /// </summary>
        [BsonElement("Timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// IP Address of the user
        /// </summary>
        [BsonElement("IPAddress")]
        public string? IPAddress { get; set; }

        /// <summary>
        /// User Agent (browser/device information)
        /// </summary>
        [BsonElement("UserAgent")]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Session ID for correlating activities in the same session
        /// </summary>
        [BsonElement("SessionId")]
        public string? SessionId { get; set; }

        /// <summary>
        /// Trace ID for distributed tracing
        /// </summary>
        [BsonElement("TraceId")]
        public string? TraceId { get; set; }

        /// <summary>
        /// Indicates if the activity was successful
        /// </summary>
        [BsonElement("IsSuccess")]
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Error message if the activity failed
        /// </summary>
        [BsonElement("ErrorMessage")]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Duration of the operation in milliseconds (if applicable)
        /// </summary>
        [BsonElement("DurationMs")]
        public long? DurationMs { get; set; }

        /// <summary>
        /// Module or feature area where the activity occurred
        /// </summary>
        [BsonElement("Module")]
        public string? Module { get; set; }

        /// <summary>
        /// Property ID if activity is property-specific
        /// </summary>
        [BsonElement("PropertyId")]
        public int? PropertyId { get; set; }

        // Constructors
        public UserActivityLog()
        {
            Timestamp = DateTime.UtcNow;
            IsSuccess = true;
        }

        public UserActivityLog(
            int userId,
            string username,
            ActivityType activityType,
            string action,
            string description,
            string? entityType = null,
            int? entityId = null,
            string? ipAddress = null,
            string? userAgent = null)
        {
            UserId = userId;
            Username = username;
            ActivityType = activityType;
            EntityType = entityType;
            EntityId = entityId;
            Action = action;
            Description = description;
            IPAddress = ipAddress;
            UserAgent = userAgent;
            Timestamp = DateTime.UtcNow;
            IsSuccess = true;
        }

        /// <summary>
        /// Creates a failed activity log entry
        /// </summary>
        public static UserActivityLog CreateFailedActivity(
            int userId,
            string username,
            ActivityType activityType,
            string action,
            string errorMessage,
            string? ipAddress = null,
            string? userAgent = null)
        {
            return new UserActivityLog
            {
                UserId = userId,
                Username = username,
                ActivityType = activityType,
                Action = action,
                Description = $"Failed: {action}",
                ErrorMessage = errorMessage,
                IsSuccess = false,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Adds metadata to the activity
        /// </summary>
        public void AddMetadata(string key, object value)
        {
            Metadata ??= new Dictionary<string, object>();
            Metadata[key] = value;
        }
    }
}
