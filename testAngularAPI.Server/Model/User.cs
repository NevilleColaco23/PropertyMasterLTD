using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace testAngularAPI.Server.Model
{
    [BsonIgnoreExtraElements]
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Username")]
        public string? Username { get; set; }

        [BsonElement("Email")]
        public string? Email { get; set; }

        public User(string username, string email)
        {
            Username = username;
            Email = email;
        }

        public User()
        {
        }
    }
}
