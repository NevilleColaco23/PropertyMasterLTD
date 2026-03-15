using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWarehouse.Application.UserActivity.DTOs;
using MyWarehouse.Application.UserActivity.Queries;

namespace MyWarehouse.WebApi.API.V1;

/// <summary>
/// Activity analytics and reporting API
/// </summary>
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{v:apiVersion}/activity/analytics")]
public class ActivityAnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ActivityAnalyticsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get overall activity analytics summary
    /// </summary>
    /// <param name="startDate">Start date filter (optional)</param>
    /// <param name="endDate">End date filter (optional)</param>
    /// <returns>Analytics summary</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ActivityAnalyticsSummaryDto), 200)]
    public async Task<ActionResult<ActivityAnalyticsSummaryDto>> GetAnalyticsSummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _mediator.Send(new GetActivityAnalyticsSummaryQuery
        {
            StartDate = startDate,
            EndDate = endDate
        });

        return Ok(result);
    }

    /// <summary>
    /// Get top active users
    /// </summary>
    /// <param name="limit">Number of users to return</param>
    /// <param name="startDate">Start date filter (optional)</param>
    /// <param name="endDate">End date filter (optional)</param>
    /// <returns>List of user statistics</returns>
    [HttpGet("top-users")]
    [ProducesResponseType(typeof(List<UserActivityStatsDto>), 200)]
    public async Task<ActionResult<List<UserActivityStatsDto>>> GetTopActiveUsers(
        [FromQuery] int limit = 10,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _mediator.Send(new GetTopActiveUsersQuery
        {
            Limit = limit,
            StartDate = startDate,
            EndDate = endDate
        });

        return Ok(result);
    }

    /// <summary>
    /// Get activity type distribution
    /// </summary>
    /// <param name="startDate">Start date filter (optional)</param>
    /// <param name="endDate">End date filter (optional)</param>
    /// <returns>Activity type statistics</returns>
    [HttpGet("distribution")]
    [ProducesResponseType(typeof(List<ActivityTypeDistributionDto>), 200)]
    public async Task<ActionResult<List<ActivityTypeDistributionDto>>> GetActivityDistribution(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _mediator.Send(new GetActivityTypeDistributionQuery
        {
            StartDate = startDate,
            EndDate = endDate
        });

        return Ok(result);
    }

    /// <summary>
    /// Get most accessed entities
    /// </summary>
    /// <param name="entityType">Entity type filter (optional)</param>
    /// <param name="limit">Number of entities to return</param>
    /// <param name="startDate">Start date filter (optional)</param>
    /// <param name="endDate">End date filter (optional)</param>
    /// <returns>Entity access statistics</returns>
    [HttpGet("top-entities")]
    [ProducesResponseType(typeof(List<EntityAccessStatsDto>), 200)]
    public async Task<ActionResult<List<EntityAccessStatsDto>>> GetMostAccessedEntities(
        [FromQuery] string? entityType = null,
        [FromQuery] int limit = 10,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _mediator.Send(new GetMostAccessedEntitiesQuery
        {
            EntityType = entityType,
            Limit = limit,
            StartDate = startDate,
            EndDate = endDate
        });

        return Ok(result);
    }

    /// <summary>
    /// Get peak usage times (hourly distribution)
    /// </summary>
    /// <param name="startDate">Start date filter (optional)</param>
    /// <param name="endDate">End date filter (optional)</param>
    /// <returns>Hourly activity statistics</returns>
    [HttpGet("peak-times")]
    [ProducesResponseType(typeof(List<PeakUsageTimeDto>), 200)]
    public async Task<ActionResult<List<PeakUsageTimeDto>>> GetPeakUsageTimes(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _mediator.Send(new GetPeakUsageTimesQuery
        {
            StartDate = startDate,
            EndDate = endDate
        });

        return Ok(result);
    }

    /// <summary>
    /// Get daily activity trends
    /// </summary>
    /// <param name="days">Number of days to include</param>
    /// <returns>Daily trend data</returns>
    [HttpGet("trends")]
    [ProducesResponseType(typeof(List<DailyActivityTrendDto>), 200)]
    public async Task<ActionResult<List<DailyActivityTrendDto>>> GetDailyTrends(
        [FromQuery] int days = 30)
    {
        var result = await _mediator.Send(new GetDailyActivityTrendsQuery
        {
            Days = days
        });

        return Ok(result);
    }

    /// <summary>
    /// Get failed login attempts
    /// </summary>
    /// <param name="limit">Number of attempts to return</param>
    /// <param name="startDate">Start date filter (optional)</param>
    /// <param name="endDate">End date filter (optional)</param>
    /// <returns>Failed login details</returns>
    [HttpGet("security/failed-logins")]
    [ProducesResponseType(typeof(List<FailedLoginAttemptDto>), 200)]
    public async Task<ActionResult<List<FailedLoginAttemptDto>>> GetFailedLogins(
        [FromQuery] int limit = 50,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _mediator.Send(new GetFailedLoginAttemptsQuery
        {
            Limit = limit,
            StartDate = startDate,
            EndDate = endDate
        });

        return Ok(result);
    }

    /// <summary>
    /// Get security alerts summary
    /// </summary>
    /// <param name="hours">Number of hours to analyze</param>
    /// <returns>Security summary</returns>
    [HttpGet("security/alerts")]
    [ProducesResponseType(typeof(SecurityAlertSummaryDto), 200)]
    public async Task<ActionResult<SecurityAlertSummaryDto>> GetSecurityAlerts(
        [FromQuery] int hours = 24)
    {
        var result = await _mediator.Send(new GetSecurityAlertsSummaryQuery
        {
            Hours = hours
        });

        return Ok(result);
    }

    /// <summary>
    /// Get performance metrics by activity type
    /// </summary>
    /// <param name="startDate">Start date filter (optional)</param>
    /// <param name="endDate">End date filter (optional)</param>
    /// <returns>Performance metrics</returns>
    [HttpGet("performance")]
    [ProducesResponseType(typeof(List<PerformanceMetricsDto>), 200)]
    public async Task<ActionResult<List<PerformanceMetricsDto>>> GetPerformanceMetrics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _mediator.Send(new GetPerformanceMetricsQuery
        {
            StartDate = startDate,
            EndDate = endDate
        });

        return Ok(result);
    }

    /// <summary>
    /// Export activities to Excel/CSV format
    /// </summary>
    /// <param name="startDate">Start date filter (optional)</param>
    /// <param name="endDate">End date filter (optional)</param>
    /// <param name="activityType">Activity type filter (optional)</param>
    /// <param name="userId">User ID filter (optional)</param>
    /// <param name="status">Status filter (Success/Failed) (optional)</param>
    /// <returns>Activity export data</returns>
    [HttpGet("export")]
    [ProducesResponseType(typeof(List<ActivityExportDto>), 200)]
    public async Task<ActionResult<List<ActivityExportDto>>> ExportActivities(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? activityType = null,
        [FromQuery] int? userId = null,
        [FromQuery] string? status = null)
    {
        var result = await _mediator.Send(new ExportActivitiesQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            ActivityType = activityType,
            UserId = userId,
            Status = status
        });

        return Ok(result);
    }
}
