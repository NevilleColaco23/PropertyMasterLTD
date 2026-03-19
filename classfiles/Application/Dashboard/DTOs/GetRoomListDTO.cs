using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MyWarehouse.Application.Dashboard.DTOs
{
    /// <summary>
    /// DTO for Room Planner - represents a room with its details
    /// </summary>
    public class GetRoomListDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("roomId")]
        public int RoomId { get; set; }

        [BsonElement("roomNumber")]
        public string RoomNumber { get; set; } = string.Empty;

        [BsonElement("roomName")]
        public string? RoomName { get; set; }

        [BsonElement("roomType")]
        public string RoomType { get; set; } = string.Empty;

        [BsonElement("propertyId")]
        public int PropertyId { get; set; }

        [BsonElement("propertyName")]
        public string PropertyName { get; set; } = string.Empty;

        [BsonElement("floor")]
        public int? Floor { get; set; }

        [BsonElement("capacity")]
        public int? Capacity { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "Available";

        [BsonElement("amenities")]
        public List<string> Amenities { get; set; } = new List<string>();

        [BsonElement("pricePerNight")]
        public decimal? PricePerNight { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;
    }
}
