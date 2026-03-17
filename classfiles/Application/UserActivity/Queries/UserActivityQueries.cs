using MediatR;
using MyWarehouse.Application.UserActivity.DTOs;
using MyWarehouse.Domain.UserActivity;
using System;

namespace MyWarehouse.Application.UserActivity.Queries
{
    /// <summary>
    /// Query to get recent activities across all users
    /// </summary>
    public record GetRecentActivitiesQuery(int Count = 50) : IRequest<List<UserActivityDTO>>;

    /// <summary>
    /// Query to get activities for a specific user
    /// </summary>
    public record GetUserActivitiesQuery(
        int UserId,
        DateTime? From = null,
        DateTime? To = null,
        int? Limit = null
    ) : IRequest<List<UserActivityDTO>>;

    /// <summary>
    /// Query to get activities by type
    /// </summary>
    public record GetActivitiesByTypeQuery(
        ActivityType ActivityType,
        DateTime? From = null,
        DateTime? To = null,
        int? Limit = null
    ) : IRequest<List<UserActivityDTO>>;

    /// <summary>
    /// Query to get entity activity history (audit trail)
    /// </summary>
    public record GetEntityActivitiesQuery(
        string EntityType,
        int EntityId,
        DateTime? From = null,
        DateTime? To = null
    ) : IRequest<List<UserActivityDTO>>;

    /// <summary>
    /// Query to get paginated activities with filters
    /// </summary>
    public record GetActivitiesPagedQuery(
        int? UserId = null,
        ActivityType? ActivityType = null,
        string? EntityType = null,
        DateTime? From = null,
        DateTime? To = null,
        int Page = 1,
        int PageSize = 50,
        string SortBy = "Timestamp",
        bool SortDescending = true
    ) : IRequest<PagedActivitiesDTO>;

    /// <summary>
    /// Query to get activity statistics
    /// </summary>
    public record GetActivityStatisticsQuery(
        DateTime? From = null,
        DateTime? To = null
    ) : IRequest<ActivityStatisticsDTO>;

    /// <summary>
    /// Query to get activity summary (for dashboard widget)
    /// </summary>
    public record GetActivitySummaryQuery(int RecentCount = 10) : IRequest<ActivitySummaryDTO>;

    /// <summary>
    /// Query to get recent activities for widget display (lightweight, only essential fields)
    /// </summary>
    public record GetActivityWidgetDataQuery(int Count = 15) : IRequest<List<ActivityWidgetDTO>>;
}
