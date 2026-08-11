using MongoDB.Bson.Serialization.Attributes;

namespace MyWarehouse.Application.Common.Users.DTO
{
    /// <summary>
    /// Lightweight user projection (including property access) used to resolve which
    /// users can be @mentioned in a post/comment for a given property.
    /// </summary>
    public class GetUserWithPropertyAccessDTO
    {
        [BsonElement("_id")]
        public int Id { get; set; }

        [BsonElement("UserName")]
        public string Username { get; set; }

        [BsonElement("Email")]
        public string Email { get; set; }

        [BsonElement("Alias")]
        public string? Alias { get; set; }

        [BsonElement("EmailConfirmed")]
        public bool EmailConfirmed { get; set; }

        [BsonElement("LockoutEnabled")]
        public bool LockoutEnabled { get; set; }

        [BsonElement("PropertyAccessList")]
        public List<PropertyAccessListDTO> PropertyAccessList { get; set; } = new();
    }

    public class PropertyAccessListDTO
    {
        [BsonElement("Id")]
        public int Id { get; set; }

        [BsonElement("IsActive")]
        public bool IsActive { get; set; }
    }
}
