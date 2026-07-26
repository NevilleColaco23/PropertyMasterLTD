using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace testAngularAPI.Server.Model
{
    [BsonIgnoreExtraElements]
    public class RoomEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("PropertyId")]
        public int PropertyId { get; set; }

        [BsonElement("RoomCode")]
        public string RoomCode { get; set; }

        [BsonElement("RoomName")]
        public string RoomName { get; set; }

        [BsonElement("Active")]
        public bool Active { get; set; }
    }
}
