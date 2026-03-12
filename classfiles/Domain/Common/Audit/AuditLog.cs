using MongoDB.Bson.Serialization.Attributes;

namespace MyWarehouse.Domain.Common.Audit
{
    public class AuditLog : IEntity<int>
    {
        [BsonId]
        public int Id { get; set; }
        
        public string CollectionName { get; set; }  // e.g., "MenuPermissions"
        public int DocumentId { get; set; }         // ID of the affected document
        public string Operation { get; set; }       // "Create", "Update", "Delete"
        public int UserId { get; set; }             // Who made the change
        public string Username { get; set; }        // For easier querying
        public DateTime Timestamp { get; set; }
        public string OldValue { get; set; }        // JSON snapshot before change
        public string NewValue { get; set; }        // JSON snapshot after change
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
    }
}
