using MediatR;
using MyWarehouse.Domain.Bookings;

namespace MyWarehouse.Application.Bookings.Commands
{
    /// <summary>
    /// Command to update booking check-in and check-out dates
    /// </summary>
    public class UpdateBookingDatesCommand : IRequest<Domain.Bookings.Bookings>
    {
        /// <summary>
        /// The booking ID to update
        /// </summary>
        public string BookingId { get; set; }

        /// <summary>
        /// New check-in date
        /// </summary>
        public DateTime NewCheckInDate { get; set; }

        /// <summary>
        /// New check-out date
        /// </summary>
        public DateTime NewCheckOutDate { get; set; }

        /// <summary>
        /// User ID performing the action
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Optional reason for date change
        /// </summary>
        public string? Reason { get; set; }

        public UpdateBookingDatesCommand(
            string bookingId, 
            DateTime newCheckInDate, 
            DateTime newCheckOutDate, 
            int userId,
            string? reason = null)
        {
            BookingId = bookingId;
            NewCheckInDate = newCheckInDate;
            NewCheckOutDate = newCheckOutDate;
            UserId = userId;
            Reason = reason;
        }
    }
}
