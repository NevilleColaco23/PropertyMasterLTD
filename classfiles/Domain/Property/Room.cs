using MongoDB.Bson.Serialization.Attributes;
using MyWarehouse.Domain.Common;

namespace MyWarehouse.Domain.Property;

public class Room : IEntity<int>
{
    [BsonId]
    public int Id { get; set; }
    
    public int PropertyId { get; set; }
    public string CompanyLogoURL { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public bool Active { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }

    // Soft delete fields
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

    public Room()
    {
    }

    public Room(int propertyId, string roomCode, string roomName, bool isActive, string companyLogoURL)
    {
        PropertyId = propertyId;
        RoomCode = roomCode;
        RoomName = roomName;
        Active = isActive;
        CompanyLogoURL = companyLogoURL;
    }
}
