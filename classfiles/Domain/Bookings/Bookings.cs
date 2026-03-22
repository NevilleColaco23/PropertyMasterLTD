using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MyWarehouse.Domain.Common;

namespace MyWarehouse.Domain.Bookings
{
    public class Bookings : IEntity<long>
    {
        [BsonId]
        public long Id { get; set; } // MongoDB's _id field (stored as Int64)

        // The custom booking ID, which is alphanumeric string
        [BsonElement("bookingId")]
        public string BookingId { get; set; }

        // References to Guest and Staff collections (Int64 in MongoDB, so long in C#)
        [BsonElement("guestId")]
        public long GuestId { get; set; }

        [BsonElement("staffId")]
        public long StaffId { get; set; }

        // Reference to the Room number (string in MongoDB)
        [BsonElement("roomNumber")]
        public string RoomNumber { get; set; }

        // Date fields
        [BsonElement("bookingDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)] // Store as UTC
        public DateTime BookingDate { get; set; }

        [BsonElement("checkInDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)] // Store as UTC
        public DateTime CheckInDate { get; set; }

        [BsonElement("checkOutDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)] // Store as UTC
        public DateTime CheckOutDate { get; set; }

        // Other booking details
        [BsonElement("numberOfGuests")]
        public int NumberOfGuests { get; set; }

        [BsonElement("totalPrice")]
        public double TotalPrice { get; set; } // Use double for floating point numbers

        // References to master collections (Int64 in MongoDB)
        [BsonElement("paymentStatusId")]
        public long PaymentStatusId { get; set; }

        [BsonElement("bookingSourceId")]
        public long BookingSourceId { get; set; }

        // Array of strings for special requests
        [BsonElement("specialRequests")]
        public List<string> SpecialRequests { get; set; }

        // Boolean for confirmation status
        [BsonElement("isConfirmed")]
        public bool IsConfirmed { get; set; }

        // Last modified timestamp
        [BsonElement("lastModified")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)] // Store as UTC
        public DateTime LastModified { get; set; }
    }
}
