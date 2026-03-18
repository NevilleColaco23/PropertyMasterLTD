using MongoDB.Bson.Serialization.Attributes;

namespace MyWarehouse.Domain.Common.Menus
{
    public class Menus : IEntity<int>
    {
        [BsonId]
        public int Id { get; set; }
        public string MenuName { get; set; }
        public int ParentMenuId { get; set; }
        public bool isActive { get; set; }
        public int Priority { get; set; } = 0; // Default priority for ordering
    }
}
