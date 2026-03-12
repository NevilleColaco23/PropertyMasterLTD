using MongoDB.Bson.Serialization.Attributes;

namespace MyWarehouse.Application.Common.Users.DTO
{
    public class GetUserDTO
    {
        [BsonElement("_id")]
        public int Id { get; set; }
        
        [BsonElement("UserName")]
        public string Username { get; set; }
        
        [BsonElement("Email")]
        public string Email { get; set; }
        
        [BsonElement("EmailConfirmed")]
        public bool EmailConfirmed { get; set; }
        
        [BsonElement("LockoutEnabled")]
        public bool LockoutEnabled { get; set; }
    }
}
