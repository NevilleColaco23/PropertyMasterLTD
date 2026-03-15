namespace MyWarehouse.Application.UserActivity.DTOs;

/// <summary>
/// Activity analytics summary
/// </summary>
public class ActivityAnalyticsSummaryDto
{
    public int TotalActivities { get; set; }
    public int UniqueUsers { get; set; }
    public int UniqueEntities { get; set; }
    public double AverageDuration { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public double SuccessRate { get; set; }
    public string MostActiveUser { get; set; } = string.Empty;
    public string MostCommonActivityType { get; set; } = string.Empty;
    public DateTime? EarliestActivity { get; set; }
    public DateTime? LatestActivity { get; set; }
}

/// <summary>
/// User activity statistics
/// </summary>
public class UserActivityStatsDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int TotalActivities { get; set; }
    public int SuccessfulActivities { get; set; }
    public int FailedActivities { get; set; }
    public double SuccessRate { get; set; }
    public DateTime LastActivityTime { get; set; }
    public List<string> TopActivityTypes { get; set; } = new();
    public List<string> MostAccessedEntities { get; set; } = new();
}

/// <summary>
/// Activity type distribution
/// </summary>
public class ActivityTypeDistributionDto
{
    public string ActivityType { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
    public double AverageDuration { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
}

/// <summary>
/// Entity access statistics
/// </summary>
public class EntityAccessStatsDto
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public int AccessCount { get; set; }
    public int UniqueUsers { get; set; }
    public DateTime FirstAccess { get; set; }
    public DateTime LastAccess { get; set; }
    public List<string> TopUsers { get; set; } = new();
}

/// <summary>
/// Peak usage time statistics
/// </summary>
public class PeakUsageTimeDto
{
    public int Hour { get; set; }
    public string HourLabel { get; set; } = string.Empty;
    public int ActivityCount { get; set; }
    public List<string> TopActivityTypes { get; set; } = new();
}

/// <summary>
/// Daily activity trend
/// </summary>
public class DailyActivityTrendDto
{
    public DateTime Date { get; set; }
    public string DateLabel { get; set; } = string.Empty;
    public int TotalActivities { get; set; }
    public int SuccessfulActivities { get; set; }
    public int FailedActivities { get; set; }
    public int UniqueUsers { get; set; }
    public double AverageDuration { get; set; }
}

/// <summary>
/// Failed login attempt details
/// </summary>
public class FailedLoginAttemptDto
{
    public int ActivityId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
}

/// <summary>
/// Security alert summary
/// </summary>
public class SecurityAlertSummaryDto
{
    public int FailedLoginCount { get; set; }
    public int SuspiciousIPCount { get; set; }
    public int MultipleFailedAttemptsCount { get; set; }
    public List<FailedLoginAttemptDto> RecentFailedLogins { get; set; } = new();
    public List<string> SuspiciousIPAddresses { get; set; } = new();
}

/// <summary>
/// Performance metrics
/// </summary>
public class PerformanceMetricsDto
{
    public string ActivityType { get; set; } = string.Empty;
    public double AverageDuration { get; set; }
    public double MinDuration { get; set; }
    public double MaxDuration { get; set; }
    public double MedianDuration { get; set; }
    public int TotalCount { get; set; }
    public int SlowRequestCount { get; set; } // > 1000ms
}

/// <summary>
/// Activity export data
/// </summary>
public class ActivityExportDto
{
    public int ActivityId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double Duration { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
