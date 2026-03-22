using System.ComponentModel.DataAnnotations;

namespace MyWarehouse.Application.Bookings.Commands
{
    /// <summary>
    /// Request to move a booking to a different room
    /// </summary>
    public class MoveBookingRequest
    {
        /// <summary>
        /// The new room number to move the booking to
        /// </summary>
        [Required(ErrorMessage = "New room number is required")]
        public string NewRoomNumber { get; set; }

        /// <summary>
        /// User ID performing the action (for activity logging)
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }
    }
}
