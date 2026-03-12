using MongoDB.Bson.Serialization.Attributes;

namespace MyWarehouse.Application.Common.Menus.DTO
{
    public class GetAllMenusDTO
    {
        [BsonElement("_id")]
        public int Id { get; set; }

        [BsonElement("label")]
        public string Label { get; set; }

        [BsonElement("path")]
        public string Path { get; set; }

        [BsonElement("order")]
        public int Order { get; set; }

        [BsonElement("hasDropdown")]
        public bool HasDropdown { get; set; }

        [BsonElement("subItems")]
        public List<SubMenuItemDTO> SubItems { get; set; }

        [BsonElement("isVisible")]
        public bool IsVisible { get; set; }
    }
}
