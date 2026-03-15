using MediatR;
using MongoDB.Driver;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Application.UserActivity.DTOs;
using MyWarehouse.Application.UserActivity.Queries;
using MyWarehouse.Domain.UserActivity;

namespace MyWarehouse.Application.UserActivity.Handlers;

/// <summary>
/// Handler for activity analytics summary query
/// </summary>
public class GetActivityAnalyticsSummaryQueryHandler : IRequestHandler<GetActivityAnalyticsSummaryQuery, ActivityAnalyticsSummaryDto>
{
    private readonly IUserActivityRepository _repository;

    public GetActivityAnalyticsSummaryQueryHandler(IUserActivityRepository repository)
    {
        _repository = repository;
    }

    public async Task<ActivityAnalyticsSummaryDto> Handle(GetActivityAnalyticsSummaryQuery request, CancellationToken cancellationToken)
    {
        var filter = Builders<UserActivityLog>.Filter.Empty;

        if (request.StartDate.HasValue)
            filter &= Builders<UserActivityLog>.Filter.Gte(x => x.Timestamp, request.StartDate.Value);

        if (request.EndDate.HasValue)
            filter &= Builders<UserActivityLog>.Filter.Lte(x => x.Timestamp, request.EndDate.Value);

        var activities = await _repository.FindAsync(filter);
        var activityList = activities.ToList();

        if (!activityList.Any())
        {
            return new ActivityAnalyticsSummaryDto();
        }

        var totalActivities = activityList.Count;
        var successCount = activityList.Count(a => a.IsSuccess);
        var failureCount = totalActivities - successCount;

        return new ActivityAnalyticsSummaryDto
        {
            TotalActivities = totalActivities,
            UniqueUsers = activityList.Select(a => a.UserId).Distinct().Count(),
            UniqueEntities = activityList.Where(a => a.EntityId.HasValue).Select(a => $"{a.EntityType}:{a.EntityId}").Distinct().Count(),
            AverageDuration = activityList.Where(a => a.DurationMs.HasValue).Average(a => (double)(a.DurationMs ?? 0)),
            SuccessCount = successCount,
            FailureCount = failureCount,
            SuccessRate = totalActivities > 0 ? (double)successCount / totalActivities * 100 : 0,
            MostActiveUser = activityList.GroupBy(a => a.Username).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? "N/A",
            MostCommonActivityType = activityList.GroupBy(a => a.ActivityType).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key.ToString() ?? "N/A",
            EarliestActivity = activityList.Min(a => a.Timestamp),
            LatestActivity = activityList.Max(a => a.Timestamp)
        };
    }
}

/// <summary>
/// Handler for top active users query
/// </summary>
public class GetTopActiveUsersQueryHandler : IRequestHandler<GetTopActiveUsersQuery, List<UserActivityStatsDto>>
{
    private readonly IUserActivityRepository _repository;

    public GetTopActiveUsersQueryHandler(IUserActivityRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<UserActivityStatsDto>> Handle(GetTopActiveUsersQuery request, CancellationToken cancellationToken)
    {
        var filter = Builders<UserActivityLog>.Filter.Empty;

        if (request.StartDate.HasValue)
            filter &= Builders<UserActivityLog>.Filter.Gte(x => x.Timestamp, request.StartDate.Value);

        if (request.EndDate.HasValue)
            filter &= Builders<UserActivityLog>.Filter.Lte(x => x.Timestamp, request.EndDate.Value);

        var activities = await _repository.FindAsync(filter);
        var activityList = activities.ToList();

        var userStats = activityList
            .GroupBy(a => new { a.UserId, a.Username })
            .Select(g =>
            {
                var userActivities = g.ToList();
                var totalCount = userActivities.Count;
                var successCount = userActivities.Count(a => a.IsSuccess);

                return new UserActivityStatsDto
                {
                    UserId = g.Key.UserId,
                    Username = g.Key.Username,
                    TotalActivities = totalCount,
                    SuccessfulActivities = successCount,
                    FailedActivities = totalCount - successCount,
                    SuccessRate = totalCount > 0 ? (double)successCount / totalCount * 100 : 0,
                    LastActivityTime = userActivities.Max(a => a.Timestamp),
                    TopActivityTypes = userActivities.GroupBy(a => a.ActivityType).OrderByDescending(ag => ag.Count()).Take(3).Select(ag => ag.Key.ToString()).ToList(),
                    MostAccessedEntities = userActivities.Where(a => a.EntityId.HasValue).GroupBy(a => $"{a.EntityType} #{a.EntityId}").OrderByDescending(eg => eg.Count()).Take(3).Select(eg => eg.Key).ToList()
                };
            })
            .OrderByDescending(s => s.TotalActivities)
            .Take(request.Limit)
            .ToList();

        return userStats;
    }
}

/// <summary>
/// Handler for activity type distribution query
/// </summary>
public class GetActivityTypeDistributionQueryHandler : IRequestHandler<GetActivityTypeDistributionQuery, List<ActivityTypeDistributionDto>>
{
    private readonly IUserActivityRepository _repository;

    public GetActivityTypeDistributionQueryHandler(IUserActivityRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ActivityTypeDistributionDto>> Handle(GetActivityTypeDistributionQuery request, CancellationToken cancellationToken)
    {
        var filter = Builders<UserActivityLog>.Filter.Empty;

        if (request.StartDate.HasValue)
            filter &= Builders<UserActivityLog>.Filter.Gte(x => x.Timestamp, request.StartDate.Value);

        if (request.EndDate.HasValue)
            filter &= Builders<UserActivityLog>.Filter.Lte(x => x.Timestamp, request.EndDate.Value);

        var activities = await _repository.FindAsync(filter);
        var activityList = activities.ToList();
        var totalCount = activityList.Count;

        var distribution = activityList
            .GroupBy(a => a.ActivityType)
            .Select(g =>
            {
                var groupActivities = g.ToList();
                var count = groupActivities.Count;

                return new ActivityTypeDistributionDto
                {
                    ActivityType = g.Key.ToString(),
                    Count = count,
                    Percentage = totalCount > 0 ? (double)count / totalCount * 100 : 0,
                    AverageDuration = groupActivities.Where(a => a.DurationMs.HasValue).Any() ? groupActivities.Where(a => a.DurationMs.HasValue).Average(a => (double)(a.DurationMs ?? 0)) : 0,
                    SuccessCount = groupActivities.Count(a => a.IsSuccess),
                    FailureCount = groupActivities.Count(a => !a.IsSuccess)
                };
            })
            .OrderByDescending(d => d.Count)
            .ToList();

        return distribution;
    }
}

/// <summary>
/// Handler for most accessed entities query
/// </summary>
public class GetMostAccessedEntitiesQueryHandler : IRequestHandler<GetMostAccessedEntitiesQuery, List<EntityAccessStatsDto>>
{
    private readonly IUserActivityRepository _repository;

    public GetMostAccessedEntitiesQueryHandler(IUserActivityRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<EntityAccessStatsDto>> Handle(GetMostAccessedEntitiesQuery request, CancellationToken cancellationToken)
    {
        var filter = Builders<UserActivityLog>.Filter.Where(x => x.EntityId != null);

        if (!string.IsNullOrEmpty(request.EntityType))
            filter &= Builders<UserActivityLog>.Filter.Eq(x => x.EntityType, request.EntityType);

        if (request.StartDate.HasValue)
            filter &= Builders<UserActivityLog>.Filter.Gte(x => x.Timestamp, request.StartDate.Value);

        if (request.EndDate.HasValue)
            filter &= Builders<UserActivityLog>.Filter.Lte(x => x.Timestamp, request.EndDate.Value);

        var activities = await _repository.FindAsync(filter);
        var activityList = activities.ToList();

        var entityStats = activityList
            .GroupBy(a => new { a.EntityType, a.EntityId })
            .Select(g =>
            {
                var entityActivities = g.ToList();

                return new EntityAccessStatsDto
                {
                    EntityType = g.Key.EntityType,
                    EntityId = g.Key.EntityId.ToString()!,
                    AccessCount = entityActivities.Count,
                    UniqueUsers = entityActivities.Select(a => a.UserId).Distinct().Count(),
                    FirstAccess = entityActivities.Min(a => a.Timestamp),
                    LastAccess = entityActivities.Max(a => a.Timestamp),
                    TopUsers = entityActivities.GroupBy(a => a.Username).OrderByDescending(ug => ug.Count()).Take(3).Select(ug => ug.Key).ToList()
                };
            })
            .OrderByDescending(s => s.AccessCount)
            .Take(request.Limit)
            .ToList();

        return entityStats;
    }
}

/// <summary>
/// Handler for peak usage times query
/// </summary>
public class GetPeakUsageTimesQueryHandler : IRequestHandler<GetPeakUsageTimesQuery, List<PeakUsageTimeDto>>
{
    private readonly IUserActivityRepository _repository;

    public GetPeakUsageTimesQueryHandler(IUserActivityRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<PeakUsageTimeDto>> Handle(GetPeakUsageTimesQuery request, CancellationToken cancellationToken)
    {
        var filter = Builders<UserActivityLog>.Filter.Empty;

        if (request.StartDate.HasValue)
            filter &= Builders<UserActivityLog>.Filter.Gte(x => x.Timestamp, request.StartDate.Value);

        if (request.EndDate.HasValue)
            filter &= Builders<UserActivityLog>.Filter.Lte(x => x.Timestamp, request.EndDate.Value);

        var activities = await _repository.FindAsync(filter);
        var activityList = activities.ToList();

        var hourlyStats = activityList
            .GroupBy(a => a.Timestamp.Hour)
            .Select(g =>
            {
                var hourActivities = g.ToList();

                return new PeakUsageTimeDto
                {
                    Hour = g.Key,
                    HourLabel = $"{g.Key:00}:00",
                    ActivityCount = hourActivities.Count,
                    TopActivityTypes = hourActivities.GroupBy(a => a.ActivityType).OrderByDescending(ag => ag.Count()).Take(3).Select(ag => ag.Key.ToString()).ToList()
                };
            })
            .OrderBy(h => h.Hour)
            .ToList();

        return hourlyStats;
    }
}

/// <summary>
/// Handler for daily activity trends query
/// </summary>
public class GetDailyActivityTrendsQueryHandler : IRequestHandler<GetDailyActivityTrendsQuery, List<DailyActivityTrendDto>>
{
    private readonly IUserActivityRepository _repository;

    public GetDailyActivityTrendsQueryHandler(IUserActivityRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<DailyActivityTrendDto>> Handle(GetDailyActivityTrendsQuery request, CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.AddDays(-request.Days).Date;
        var filter = Builders<UserActivityLog>.Filter.Gte(x => x.Timestamp, startDate);

        var activities = await _repository.FindAsync(filter);
        var activityList = activities.ToList();

        var dailyTrends = activityList
            .GroupBy(a => a.Timestamp.Date)
            .Select(g =>
            {
                var dayActivities = g.ToList();
                var totalCount = dayActivities.Count;
                var successCount = dayActivities.Count(a => a.IsSuccess);

                return new DailyActivityTrendDto
                {
                    Date = g.Key,
                    DateLabel = g.Key.ToString("MMM dd"),
                    TotalActivities = totalCount,
                    SuccessfulActivities = successCount,
                    FailedActivities = totalCount - successCount,
                    UniqueUsers = dayActivities.Select(a => a.UserId).Distinct().Count(),
                    AverageDuration = dayActivities.Where(a => a.DurationMs.HasValue).Any() ? dayActivities.Where(a => a.DurationMs.HasValue).Average(a => (double)(a.DurationMs ?? 0)) : 0
                };
            })
            .OrderBy(t => t.Date)
            .ToList();

        return dailyTrends;
    }
}

/// <summary>
/// Handler for failed login attempts query
/// </summary>
public class GetFailedLoginAttemptsQueryHandler : IRequestHandler<GetFailedLoginAttemptsQuery, List<FailedLoginAttemptDto>>
{
    private readonly IUserActivityRepository _repository;

    public GetFailedLoginAttemptsQueryHandler(IUserActivityRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<FailedLoginAttemptDto>> Handle(GetFailedLoginAttemptsQuery request, CancellationToken cancellationToken)
    {
        var filter = Builders<UserActivityLog>.Filter.Eq(x => x.ActivityType, ActivityType.Login) &
                     Builders<UserActivityLog>.Filter.Eq(x => x.IsSuccess, false);

        if (request.StartDate.HasValue)
            filter &= Builders<UserActivityLog>.Filter.Gte(x => x.Timestamp, request.StartDate.Value);

        if (request.EndDate.HasValue)
            filter &= Builders<UserActivityLog>.Filter.Lte(x => x.Timestamp, request.EndDate.Value);

        var failedLogins = await _repository.FindAsync(filter);

        var failedAttempts = failedLogins
            .OrderByDescending(a => a.Timestamp)
            .Take(request.Limit)
            .Select(a => new FailedLoginAttemptDto
            {
                ActivityId = a.Id,
                Username = a.Username,
                IPAddress = a.IPAddress ?? "Unknown",
                UserAgent = a.UserAgent ?? "Unknown",
                Timestamp = a.Timestamp,
                ErrorMessage = a.ErrorMessage ?? "Login failed",
                AttemptCount = 1
            })
            .ToList();

        // Group by IP and count attempts
        var groupedByIP = failedAttempts
            .GroupBy(f => f.IPAddress)
            .Where(g => g.Count() > 1)
            .ToList();

        foreach (var group in groupedByIP)
        {
            foreach (var attempt in group)
            {
                attempt.AttemptCount = group.Count();
            }
        }

        return failedAttempts;
    }
}

/// <summary>
/// Handler for security alerts summary query
/// </summary>
public class GetSecurityAlertsSummaryQueryHandler : IRequestHandler<GetSecurityAlertsSummaryQuery, SecurityAlertSummaryDto>
{
    private readonly IUserActivityRepository _repository;

    public GetSecurityAlertsSummaryQueryHandler(IUserActivityRepository repository)
    {
        _repository = repository;
    }

    public async Task<SecurityAlertSummaryDto> Handle(GetSecurityAlertsSummaryQuery request, CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.AddHours(-request.Hours);
        var filter = Builders<UserActivityLog>.Filter.Gte(x => x.Timestamp, startDate) &
                     Builders<UserActivityLog>.Filter.Eq(x => x.ActivityType, ActivityType.Login) &
                     Builders<UserActivityLog>.Filter.Eq(x => x.IsSuccess, false);

        var failedLogins = await _repository.FindAsync(filter);
        var failedLoginsList = failedLogins.ToList();

        var recentFailures = failedLoginsList
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .Select(a => new FailedLoginAttemptDto
            {
                ActivityId = a.Id,
                Username = a.Username,
                IPAddress = a.IPAddress ?? "Unknown",
                UserAgent = a.UserAgent ?? "Unknown",
                Timestamp = a.Timestamp,
                ErrorMessage = a.ErrorMessage ?? "Login failed",
                AttemptCount = 1
            })
            .ToList();

        var ipGroups = failedLoginsList
            .Where(f => !string.IsNullOrEmpty(f.IPAddress))
            .GroupBy(f => f.IPAddress)
            .Where(g => g.Count() >= 3) // 3+ failures from same IP = suspicious
            .Select(g => g.Key!)
            .ToList();

        return new SecurityAlertSummaryDto
        {
            FailedLoginCount = failedLoginsList.Count,
            SuspiciousIPCount = ipGroups.Count,
            MultipleFailedAttemptsCount = failedLoginsList.GroupBy(f => f.Username).Count(g => g.Count() >= 3),
            RecentFailedLogins = recentFailures,
            SuspiciousIPAddresses = ipGroups
        };
    }
}

/// <summary>
/// Handler for performance metrics query
/// </summary>
public class GetPerformanceMetricsQueryHandler : IRequestHandler<GetPerformanceMetricsQuery, List<PerformanceMetricsDto>>
{
    private readonly IUserActivityRepository _repository;

    public GetPerformanceMetricsQueryHandler(IUserActivityRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<PerformanceMetricsDto>> Handle(GetPerformanceMetricsQuery request, CancellationToken cancellationToken)
    {
        var filter = Builders<UserActivityLog>.Filter.Empty;

        if (request.StartDate.HasValue)
            filter &= Builders<UserActivityLog>.Filter.Gte(x => x.Timestamp, request.StartDate.Value);

        if (request.EndDate.HasValue)
            filter &= Builders<UserActivityLog>.Filter.Lte(x => x.Timestamp, request.EndDate.Value);

        var activities = await _repository.FindAsync(filter);
        var activityList = activities.ToList();

        var performanceMetrics = activityList
            .GroupBy(a => a.ActivityType)
            .Select(g =>
            {
                var durations = g.Where(a => a.DurationMs.HasValue).Select(a => (double)(a.DurationMs ?? 0)).OrderBy(d => d).ToList();
                var count = durations.Count;

                return new PerformanceMetricsDto
                {
                    ActivityType = g.Key.ToString(),
                    AverageDuration = count > 0 ? durations.Average() : 0,
                    MinDuration = count > 0 ? durations.Min() : 0,
                    MaxDuration = count > 0 ? durations.Max() : 0,
                    MedianDuration = count > 0 ? durations[count / 2] : 0,
                    TotalCount = g.Count(),
                    SlowRequestCount = durations.Count(d => d > 1000) // > 1 second
                };
            })
            .OrderByDescending(m => m.AverageDuration)
            .ToList();

        return performanceMetrics;
    }
}

/// <summary>
/// Handler for export activities query
/// </summary>
public class ExportActivitiesQueryHandler : IRequestHandler<ExportActivitiesQuery, List<ActivityExportDto>>
{
    private readonly IUserActivityRepository _repository;

    public ExportActivitiesQueryHandler(IUserActivityRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ActivityExportDto>> Handle(ExportActivitiesQuery request, CancellationToken cancellationToken)
    {
        var filter = Builders<UserActivityLog>.Filter.Empty;

        if (request.StartDate.HasValue)
            filter &= Builders<UserActivityLog>.Filter.Gte(x => x.Timestamp, request.StartDate.Value);

        if (request.EndDate.HasValue)
            filter &= Builders<UserActivityLog>.Filter.Lte(x => x.Timestamp, request.EndDate.Value);

        if (!string.IsNullOrEmpty(request.ActivityType))
            filter &= Builders<UserActivityLog>.Filter.Eq(x => x.ActivityType, Enum.Parse<ActivityType>(request.ActivityType));

        if (request.UserId.HasValue)
            filter &= Builders<UserActivityLog>.Filter.Eq(x => x.UserId, request.UserId.Value);

        if (!string.IsNullOrEmpty(request.Status))
        {
            var isSuccess = request.Status.Equals("Success", StringComparison.OrdinalIgnoreCase);
            filter &= Builders<UserActivityLog>.Filter.Eq(x => x.IsSuccess, isSuccess);
        }

        var activities = await _repository.FindAsync(filter);

        var exportData = activities
            .OrderByDescending(a => a.Timestamp)
            .Select(a => new ActivityExportDto
            {
                ActivityId = a.Id,
                Username = a.Username,
                ActivityType = a.ActivityType.ToString(),
                EntityType = a.EntityType ?? "",
                EntityId = a.EntityId?.ToString() ?? "",
                Description = a.Description ?? "",
                IPAddress = a.IPAddress ?? "",
                Timestamp = a.Timestamp,
                Duration = a.DurationMs ?? 0,
                Status = a.IsSuccess ? "Success" : "Failed",
                ErrorMessage = a.ErrorMessage ?? ""
            })
            .ToList();

        return exportData;
    }
}
