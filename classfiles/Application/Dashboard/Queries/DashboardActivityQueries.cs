using MyWarehouse.Application.Common.Dependencies.DataAccess;

namespace MyWarehouse.Application.Dashboard.Queries
{
    /// <summary>
    /// Query to get recent activity
    /// </summary>
    public class GetRecentActivityQuery : IRequest<List<ActivityItemResponse>>
    {
        public int UserId { get; set; }
        public int Limit { get; set; } = 10;
    }

    /// <summary>
    /// Query to get recent bookings
    /// </summary>
    public class GetRecentBookingsQuery : IRequest<List<RecentBookingResponse>>
    {
        public int UserId { get; set; }
        public int Limit { get; set; } = 10;
        public List<int> PropertyIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// Query to get calendar events
    /// </summary>
    public class GetCalendarEventsQuery : IRequest<List<CalendarEventResponse>>
    {
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<int> PropertyIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// Query to get booking trends for charts
    /// </summary>
    public class GetBookingTrendsQuery : IRequest<BookingTrendsResponse>
    {
        public int UserId { get; set; }
        public int DaysBack { get; set; } = 30; // Default to last 30 days
        public string GroupBy { get; set; } = "day"; // "day", "week", "month"
        public List<int> PropertyIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// Activity item response
    /// </summary>
    public class ActivityItemResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Icon { get; set; } = "circle";
        public string IconColor { get; set; } = "#1976d2";
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Metadata { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Recent booking response
    /// </summary>
    public class RecentBookingResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Icon { get; set; } = "hotel";
        public string IconColor { get; set; } = "#667eea";
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public string Status { get; set; } = string.Empty;
        public double TotalPrice { get; set; }
    }

    /// <summary>
    /// Calendar event response
    /// </summary>
    public class CalendarEventResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public string Color { get; set; } = "#1976d2";
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Booking trends response for chart widget
    /// </summary>
    public class BookingTrendsResponse
    {
        public string Title { get; set; } = "Booking Trends";
        public List<string> Labels { get; set; } = new();
        public List<ChartDataset> Datasets { get; set; } = new();
        public string ChartType { get; set; } = "line";
    }

    /// <summary>
    /// Chart dataset
    /// </summary>
    public class ChartDataset
    {
        public string Label { get; set; } = string.Empty;
        public List<int> Data { get; set; } = new();
        public string BackgroundColor { get; set; } = "#1976d2";
        public string BorderColor { get; set; } = "#1976d2";
        public int BorderWidth { get; set; } = 2;
    }
}
