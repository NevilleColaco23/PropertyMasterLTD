using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace testAngularAPI.Server.Model
{
    [BsonIgnoreExtraElements]
    public class Guest
    {
        // Mongo _id present in DB but we keep a numeric guestId as the business identifier
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("guestId")]
        public long GuestId { get; set; }

        [BsonElement("firstName")]
        public string FirstName { get; set; }

        [BsonElement("lastName")]
        public string LastName { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("phoneNumber")]
        public string PhoneNumber { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; }
    }
}
