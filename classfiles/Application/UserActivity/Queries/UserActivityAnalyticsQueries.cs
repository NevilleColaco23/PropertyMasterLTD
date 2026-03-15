using MediatR;
using MyWarehouse.Application.UserActivity.DTOs;

namespace MyWarehouse.Application.UserActivity.Queries;

/// <summary>
/// Get overall activity analytics summary
/// </summary>
public class GetActivityAnalyticsSummaryQuery : IRequest<ActivityAnalyticsSummaryDto>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// Get top active users
/// </summary>
public class GetTopActiveUsersQuery : IRequest<List<UserActivityStatsDto>>
{
    public int Limit { get; set; } = 10;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// Get activity type distribution
/// </summary>
public class GetActivityTypeDistributionQuery : IRequest<List<ActivityTypeDistributionDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// Get most accessed entities
/// </summary>
public class GetMostAccessedEntitiesQuery : IRequest<List<EntityAccessStatsDto>>
{
    public string? EntityType { get; set; }
    public int Limit { get; set; } = 10;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// Get peak usage times (hourly distribution)
/// </summary>
public class GetPeakUsageTimesQuery : IRequest<List<PeakUsageTimeDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// Get daily activity trends
/// </summary>
public class GetDailyActivityTrendsQuery : IRequest<List<DailyActivityTrendDto>>
{
    public int Days { get; set; } = 30;
}

/// <summary>
/// Get failed login attempts
/// </summary>
public class GetFailedLoginAttemptsQuery : IRequest<List<FailedLoginAttemptDto>>
{
    public int Limit { get; set; } = 50;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// Get security alerts summary
/// </summary>
public class GetSecurityAlertsSummaryQuery : IRequest<SecurityAlertSummaryDto>
{
    public int Hours { get; set; } = 24;
}

/// <summary>
/// Get performance metrics by activity type
/// </summary>
public class GetPerformanceMetricsQuery : IRequest<List<PerformanceMetricsDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// Export activities to Excel/CSV
/// </summary>
public class ExportActivitiesQuery : IRequest<List<ActivityExportDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ActivityType { get; set; }
    public int? UserId { get; set; }
    public string? Status { get; set; }
}
