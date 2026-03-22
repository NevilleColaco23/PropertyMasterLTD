using MediatR;
using MyWarehouse.Domain.Bookings;

namespace MyWarehouse.Application.Bookings.Commands
{
    /// <summary>
    /// Command to cancel a booking
    /// </summary>
    public class CancelBookingCommand : IRequest<Domain.Bookings.Bookings>
    {
        /// <summary>
        /// The booking ID to cancel
        /// </summary>
        public string BookingId { get; set; }

        /// <summary>
        /// User ID performing the cancellation
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Optional cancellation reason
        /// </summary>
        public string? CancellationReason { get; set; }

        public CancelBookingCommand(string bookingId, int userId, string? cancellationReason = null)
        {
            BookingId = bookingId;
            UserId = userId;
            CancellationReason = cancellationReason;
        }
    }
}
