using System.ComponentModel.DataAnnotations;

namespace MyWarehouse.Application.Bookings.Commands
{
    /// <summary>
    /// Request to update booking check-in and check-out dates
    /// </summary>
    public class UpdateBookingDatesRequest
    {
        /// <summary>
        /// New check-in date
        /// </summary>
        [Required(ErrorMessage = "Check-in date is required")]
        public DateTime NewCheckInDate { get; set; }

        /// <summary>
        /// New check-out date
        /// </summary>
        [Required(ErrorMessage = "Check-out date is required")]
        public DateTime NewCheckOutDate { get; set; }

        /// <summary>
        /// User ID performing the action (for activity logging)
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        /// <summary>
        /// Optional: Reason for date change
        /// </summary>
        public string? Reason { get; set; }
    }
}
