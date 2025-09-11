using MongoDB.Bson;
using MyWarehouse.Application.Models;

namespace MyWarehouse.Application.Common.Bookings
{
    public class GetBookingsListDTO
    {
        public long _id { get; set; } 

        public string bookingId { get; set; }

        public long guestId { get; set; }

        public long StaffId { get; set; }

        public string roomNumber { get; set; }

        public DateTime bookingDate { get; set; }

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        public int numberOfGuests { get; set; }

        public double totalPrice { get; set; } 

        public string PaymentStatusId { get; set; }

        public string BookingSourceId { get; set; }

        public List<string> SpecialRequests { get; set; }

        public bool isConfirmed { get; set; }

        public DateTime LastModified { get; set; }
    }
}
