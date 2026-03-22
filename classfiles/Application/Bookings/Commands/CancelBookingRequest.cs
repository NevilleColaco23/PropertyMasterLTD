using System.ComponentModel.DataAnnotations;

namespace MyWarehouse.Application.Bookings.Commands
{
    /// <summary>
    /// Request to cancel a booking
    /// </summary>
    public class CancelBookingRequest
    {
        /// <summary>
        /// User ID performing the cancellation (for activity logging)
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        /// <summary>
        /// Optional: Reason for cancellation
        /// </summary>
        public string? CancellationReason { get; set; }
    }
}
