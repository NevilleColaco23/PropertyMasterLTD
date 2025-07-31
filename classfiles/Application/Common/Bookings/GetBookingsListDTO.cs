using MongoDB.Bson;

namespace MyWarehouse.Application.Common.Bookings
{
    public class GetBookingsListDTO
    {
        public ObjectId _id { get; set; } 

        public string BookingId { get; set; }

        public long GuestId { get; set; }

        public long StaffId { get; set; }

        public string RoomNumber { get; set; }

        public DateTime BookingDate { get; set; }

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        public int NumberOfGuests { get; set; }

        public double TotalPrice { get; set; } 

        public string PaymentStatusId { get; set; }

        public string BookingSourceId { get; set; }

        public List<string> SpecialRequests { get; set; }

        public bool IsConfirmed { get; set; }

        public DateTime LastModified { get; set; }
    }
}
