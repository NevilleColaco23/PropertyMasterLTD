using MongoDB.Bson.Serialization.Attributes;
using MyWarehouse.Domain.Common;

namespace MyWarehouse.Domain.Property;

public class RoomAudit : IEntity<int>
{
    [BsonId]
    public int Id { get; set; }

    public int PropertyId { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public string CompanyLogoURL { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public bool Active { get; set; }

    // Original audit fields
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }

    // Deletion audit fields
    public DateTime DeletedAt { get; set; }
    public int DeletedBy { get; set; }
    public string DeleteReason { get; set; } = string.Empty;

    public RoomAudit()
    {
    }
}
