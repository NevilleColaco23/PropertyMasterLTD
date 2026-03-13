using MongoDB.Bson.Serialization.Attributes;
using MyWarehouse.Domain.Common;

namespace MyWarehouse.Domain.Property;

public class PropertyAudit : IEntity<int>
{
    [BsonId]
    public int Id { get; set; }

    [BsonElement("Name")]
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; }

    [BsonElement("Rooms")]
    public List<PropertyAuditRoom> Rooms { get; set; } = new();
    public string CompanyLogoURL { get; set; } = string.Empty;
    public string PropertyCode { get; set; } = string.Empty;

    // Original audit fields
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }

    // Deletion audit fields
    public DateTime DeletedAt { get; set; }
    public int DeletedBy { get; set; }
    public string DeleteReason { get; set; } = string.Empty;

    public PropertyAudit()
    {
    }

    public class PropertyAuditRoom
    {
        [BsonId]
        public int Id { get; set; }
        public string CompanyLogoURL { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public bool Active { get; set; }

        public PropertyAuditRoom()
        {
        }

        public PropertyAuditRoom(int id, string roomCode, string roomName, bool active, string companyLogoURL)
        {
            Id = id;
            RoomCode = roomCode;
            RoomName = roomName;
            Active = active;
            CompanyLogoURL = companyLogoURL;
        }
    }
}
