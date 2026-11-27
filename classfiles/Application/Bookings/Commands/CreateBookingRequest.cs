using System.ComponentModel.DataAnnotations;

namespace MyWarehouse.Application.Bookings.Commands
{
    /// <summary>
    /// Request body for creating a new booking
    /// </summary>
    public class CreateBookingRequest
    {
        public int? PropertyId { get; set; }

        [Required(ErrorMessage = "Guest is required")]
        public long GuestId { get; set; }

        [Required(ErrorMessage = "Room number is required")]
        public string RoomNumber { get; set; }

        [Required(ErrorMessage = "Check-in date is required")]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Check-out date is required")]
        public DateTime CheckOutDate { get; set; }

        [Range(1, 20, ErrorMessage = "Number of guests must be between 1 and 20")]
        public int NumberOfGuests { get; set; } = 1;

        [Range(0, double.MaxValue, ErrorMessage = "Total price must be a positive value")]
        public double TotalPrice { get; set; }

        [Required(ErrorMessage = "Payment status is required")]
        public long PaymentStatusId { get; set; }

        [Required(ErrorMessage = "Booking source is required")]
        public long BookingSourceId { get; set; }

        public string? SpecialRequests { get; set; }

        public bool IsConfirmed { get; set; } = true;

        /// <summary>
        /// User ID performing the action (for activity logging)
        /// </summary>
        public int UserId { get; set; }
    }
}
