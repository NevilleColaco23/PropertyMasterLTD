using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace MyWarehouse.Application.Models
{
    public class Log
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Url { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
