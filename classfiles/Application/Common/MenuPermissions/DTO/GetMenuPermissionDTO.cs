using MongoDB.Bson.Serialization.Attributes;

namespace MyWarehouse.Application.Common.MenuPermissions
{
    public class GetMenuPermissionDTO
    {
        [BsonElement("_id")]
        public int Id { get; set; }

        [BsonElement("UserId")]
        public int UserId { get; set; }

        [BsonElement("MenuID")]
        public int MenuId { get; set; }

        public string MenuLabel { get; set; }  // This won't come from DB, we'll need to join with Menus

        [BsonElement("AccessLevel")]
        public string AccessLevel { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; }

        [BsonElement("From")]
        public DateTime From { get; set; }

        [BsonElement("To")]
        public DateTime To { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("CreatedBy")]
        public int CreatedBy { get; set; }
    }
}
