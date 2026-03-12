using MongoDB.Bson.Serialization.Attributes;

namespace MyWarehouse.Domain.Common.Menus;

public class MenusPermissions : IEntity<int>
{
    [BsonId]
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MenuID { get; set; }
    public string AccessLevel { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
}
