using MediatR;
using MyWarehouse.Domain.Bookings;

namespace MyWarehouse.Application.Bookings.Commands
{
    /// <summary>
    /// Command to move a booking to a different room
    /// </summary>
    public class MoveBookingCommand : IRequest<Domain.Bookings.Bookings>
    {
        /// <summary>
        /// The booking ID to move
        /// </summary>
        public string BookingId { get; set; }

        /// <summary>
        /// The new room number to move to
        /// </summary>
        public string NewRoomNumber { get; set; }

        /// <summary>
        /// User ID performing the action
        /// </summary>
        public int UserId { get; set; }

        public MoveBookingCommand(string bookingId, string newRoomNumber, int userId)
        {
            BookingId = bookingId;
            NewRoomNumber = newRoomNumber;
            UserId = userId;
        }
    }
}
