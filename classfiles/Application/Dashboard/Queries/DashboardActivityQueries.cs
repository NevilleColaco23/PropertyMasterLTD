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
    /// Query to get calendar events
    /// </summary>
    public class GetCalendarEventsQuery : IRequest<List<CalendarEventResponse>>
    {
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
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
}
