using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MyWarehouse.Domain.Common;

namespace MyWarehouse.Domain.System_Related.System_Messages;

public class SystemMessages : IEntity<ObjectId>
{
    public ObjectId Id { get; set; } // Represents MongoDB's default _id

    [BsonElement("message")]
    public string message { get; set; }

    [BsonElement("priority")]
    public string priority { get; set; }

    [BsonElement("timeFrom")]
    public DateTime timeFrom { get; set; }

    [BsonElement("timeTo")]
    public DateTime timeTo { get; set; }
}
