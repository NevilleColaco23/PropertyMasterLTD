using MediatR;
using MyWarehouse.Application.Dashboard.DTOs;

namespace MyWarehouse.Application.Dashboard.Queries
{
    /// <summary>
    /// Query to get bookings with guest details for room planner
    /// </summary>
    public class GetBookingsWithGuestsQuery : IRequest<List<BookingWithGuestDTO>>
    {
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<int> PropertyIds { get; set; } = new List<int>();
    }
}
