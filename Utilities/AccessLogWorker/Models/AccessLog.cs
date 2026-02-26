using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AccessLogWorker.Models;

public class AccessLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public DateTime Timestamp { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string ClientIp { get; set; } = string.Empty;
    public long DurationMs { get; set; }
}
