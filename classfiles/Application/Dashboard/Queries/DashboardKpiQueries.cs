using MyWarehouse.Application.Common.Dependencies.DataAccess;

namespace MyWarehouse.Application.Dashboard.Queries
{
    /// <summary>
    /// Query to get total properties count
    /// </summary>
    public class GetTotalPropertiesQuery : IRequest<KpiValueResponse>
    {
        public int UserId { get; set; }
    }

    /// <summary>
    /// Query to get total rooms count
    /// </summary>
    public class GetTotalRoomsQuery : IRequest<KpiValueResponse>
    {
        public int UserId { get; set; }
    }

    /// <summary>
    /// Query to get bookings today count
    /// </summary>
    public class GetBookingsTodayQuery : IRequest<KpiValueResponse>
    {
        public int UserId { get; set; }
        public List<int> PropertyIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// Query to get occupancy rate
    /// </summary>
    public class GetOccupancyRateQuery : IRequest<KpiValueResponse>
    {
        public int UserId { get; set; }
    }

    /// <summary>
    /// Response for KPI value queries
    /// </summary>
    public class KpiValueResponse
    {
        public string WidgetId { get; set; } = string.Empty;
        public object Value { get; set; } = 0;
        public bool ShowTrend { get; set; }
        public double? TrendValue { get; set; }
        public string? TrendDirection { get; set; } // "up" or "down"
        public DateTime CalculatedAt { get; set; }
    }
}
