using MongoDB.Bson.Serialization.Attributes;

namespace MyWarehouse.Domain.Common.Menus;

public class MenusPermissions : IEntity<int>
{
    [BsonId]
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MenuID { get; set; }
    public string AccessLevel { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}
