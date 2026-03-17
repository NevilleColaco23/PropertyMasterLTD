using MediatR;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Application.UserActivity.DTOs;
using MyWarehouse.Application.UserActivity.Queries;
using MyWarehouse.Domain.UserActivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyWarehouse.Application.UserActivity.Handlers
{
    /// <summary>
    /// Handler for GetRecentActivitiesQuery
    /// </summary>
    public class GetRecentActivitiesQueryHandler : IRequestHandler<GetRecentActivitiesQuery, List<UserActivityDTO>>
    {
        private readonly IUserActivityRepository _repository;

        public GetRecentActivitiesQueryHandler(IUserActivityRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserActivityDTO>> Handle(GetRecentActivitiesQuery request, CancellationToken cancellationToken)
        {
            var activities = await _repository.GetRecentActivitiesAsync(request.Count);
            return activities.Select(MapToDTO).ToList();
        }

        private static UserActivityDTO MapToDTO(UserActivityLog activity)
        {
            return new UserActivityDTO
            {
                ActivityId = activity.Id,
                UserId = activity.UserId,
                Username = activity.Username,
                ActivityType = activity.ActivityType.ToString(),
                EntityType = activity.EntityType,
                EntityId = activity.EntityId,
                Action = activity.Action,
                Metadata = activity.Metadata,
                Timestamp = activity.Timestamp,
                IPAddress = activity.IPAddress,
                IsSuccess = activity.IsSuccess,
                ErrorMessage = activity.ErrorMessage,
                DurationMs = activity.DurationMs,
                Module = activity.Module
            };
        }
    }

    /// <summary>
    /// Handler for GetUserActivitiesQuery
    /// </summary>
    public class GetUserActivitiesQueryHandler : IRequestHandler<GetUserActivitiesQuery, List<UserActivityDTO>>
    {
        private readonly IUserActivityRepository _repository;

        public GetUserActivitiesQueryHandler(IUserActivityRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserActivityDTO>> Handle(GetUserActivitiesQuery request, CancellationToken cancellationToken)
        {
            var activities = await _repository.GetUserActivitiesAsync(
                request.UserId,
                request.From,
                request.To,
                request.Limit
            );

            return activities.Select(MapToDTO).ToList();
        }

        private static UserActivityDTO MapToDTO(UserActivityLog activity)
        {
            return new UserActivityDTO
            {
                ActivityId = activity.Id,
                UserId = activity.UserId,
                Username = activity.Username,
                ActivityType = activity.ActivityType.ToString(),
                EntityType = activity.EntityType,
                EntityId = activity.EntityId,
                Action = activity.Action,
                Metadata = activity.Metadata,
                Timestamp = activity.Timestamp,
                IPAddress = activity.IPAddress,
                IsSuccess = activity.IsSuccess,
                ErrorMessage = activity.ErrorMessage,
                DurationMs = activity.DurationMs,
                Module = activity.Module
            };
        }
    }

    /// <summary>
    /// Handler for GetActivitiesByTypeQuery
    /// </summary>
    public class GetActivitiesByTypeQueryHandler : IRequestHandler<GetActivitiesByTypeQuery, List<UserActivityDTO>>
    {
        private readonly IUserActivityRepository _repository;

        public GetActivitiesByTypeQueryHandler(IUserActivityRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserActivityDTO>> Handle(GetActivitiesByTypeQuery request, CancellationToken cancellationToken)
        {
            var activities = await _repository.GetActivitiesByTypeAsync(
                request.ActivityType,
                request.From,
                request.To,
                request.Limit
            );

            return activities.Select(MapToDTO).ToList();
        }

        private static UserActivityDTO MapToDTO(UserActivityLog activity)
        {
            return new UserActivityDTO
            {
                ActivityId = activity.Id,
                UserId = activity.UserId,
                Username = activity.Username,
                ActivityType = activity.ActivityType.ToString(),
                EntityType = activity.EntityType,
                EntityId = activity.EntityId,
                Action = activity.Action,
                Metadata = activity.Metadata,
                Timestamp = activity.Timestamp,
                IPAddress = activity.IPAddress,
                IsSuccess = activity.IsSuccess,
                ErrorMessage = activity.ErrorMessage,
                DurationMs = activity.DurationMs,
                Module = activity.Module
            };
        }
    }

    /// <summary>
    /// Handler for GetEntityActivitiesQuery
    /// </summary>
    public class GetEntityActivitiesQueryHandler : IRequestHandler<GetEntityActivitiesQuery, List<UserActivityDTO>>
    {
        private readonly IUserActivityRepository _repository;

        public GetEntityActivitiesQueryHandler(IUserActivityRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserActivityDTO>> Handle(GetEntityActivitiesQuery request, CancellationToken cancellationToken)
        {
            var activities = await _repository.GetEntityActivitiesAsync(
                request.EntityType,
                request.EntityId,
                request.From,
                request.To
            );

            return activities.Select(MapToDTO).ToList();
        }

        private static UserActivityDTO MapToDTO(UserActivityLog activity)
        {
            return new UserActivityDTO
            {
                ActivityId = activity.Id,
                UserId = activity.UserId,
                Username = activity.Username,
                ActivityType = activity.ActivityType.ToString(),
                EntityType = activity.EntityType,
                EntityId = activity.EntityId,
                Action = activity.Action,
                Metadata = activity.Metadata,
                Timestamp = activity.Timestamp,
                IPAddress = activity.IPAddress,
                IsSuccess = activity.IsSuccess,
                ErrorMessage = activity.ErrorMessage,
                DurationMs = activity.DurationMs,
                Module = activity.Module
            };
        }
    }

    /// <summary>
    /// Handler for GetActivitiesPagedQuery
    /// </summary>
    public class GetActivitiesPagedQueryHandler : IRequestHandler<GetActivitiesPagedQuery, PagedActivitiesDTO>
    {
        private readonly IUserActivityRepository _repository;

        public GetActivitiesPagedQueryHandler(IUserActivityRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedActivitiesDTO> Handle(GetActivitiesPagedQuery request, CancellationToken cancellationToken)
        {
            var (activities, totalCount) = await _repository.GetActivitiesPagedAsync(
                request.UserId,
                request.ActivityType,
                request.EntityType,
                request.From,
                request.To,
                request.Page,
                request.PageSize,
                request.SortBy,
                request.SortDescending
            );

            return new PagedActivitiesDTO
            {
                Activities = activities.Select(MapToDTO).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        private static UserActivityDTO MapToDTO(UserActivityLog activity)
        {
            return new UserActivityDTO
            {
                ActivityId = activity.Id,
                UserId = activity.UserId,
                Username = activity.Username,
                ActivityType = activity.ActivityType.ToString(),
                EntityType = activity.EntityType,
                EntityId = activity.EntityId,
                Action = activity.Action,
                Metadata = activity.Metadata,
                Timestamp = activity.Timestamp,
                IPAddress = activity.IPAddress,
                IsSuccess = activity.IsSuccess,
                ErrorMessage = activity.ErrorMessage,
                DurationMs = activity.DurationMs,
                Module = activity.Module
            };
        }
    }

    /// <summary>
    /// Handler for GetActivityStatisticsQuery
    /// </summary>
    public class GetActivityStatisticsQueryHandler : IRequestHandler<GetActivityStatisticsQuery, ActivityStatisticsDTO>
    {
        private readonly IUserActivityRepository _repository;

        public GetActivityStatisticsQueryHandler(IUserActivityRepository repository)
        {
            _repository = repository;
        }

        public async Task<ActivityStatisticsDTO> Handle(GetActivityStatisticsQuery request, CancellationToken cancellationToken)
        {
            var stats = await _repository.GetActivityStatisticsAsync(request.From, request.To);
            var topUsers = await _repository.GetMostActiveUsersAsync(10, request.From, request.To);
            var activityCounts = await _repository.GetActivityCountByTypeAsync(request.From, request.To);

            return new ActivityStatisticsDTO
            {
                TotalActivities = (long)stats["TotalActivities"],
                UniqueUsers = (int)stats["UniqueUsers"],
                ActivityTypeBreakdown = activityCounts.ToDictionary(
                    kvp => kvp.Key.ToString(),
                    kvp => kvp.Value
                ),
                TopUsers = topUsers.Select(u => new TopUserActivityDTO
                {
                    Username = u.Username,
                    ActivityCount = u.ActivityCount,
                    MostCommonActivity = "Various" // Can be enhanced
                }).ToList(),
                FromDate = request.From,
                ToDate = request.To,
                Period = request.From.HasValue && request.To.HasValue
                    ? $"{request.From:yyyy-MM-dd} to {request.To:yyyy-MM-dd}"
                    : "All time"
            };
        }
    }

    /// <summary>
    /// Handler for GetActivitySummaryQuery (Dashboard Widget)
    /// </summary>
    public class GetActivitySummaryQueryHandler : IRequestHandler<GetActivitySummaryQuery, ActivitySummaryDTO>
    {
        private readonly IUserActivityRepository _repository;

        public GetActivitySummaryQueryHandler(IUserActivityRepository repository)
        {
            _repository = repository;
        }

        public async Task<ActivitySummaryDTO> Handle(GetActivitySummaryQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var startOfToday = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
            var startOfWeek = startOfToday.AddDays(-(int)now.DayOfWeek);
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            // Get recent activities
            var recentActivities = await _repository.GetRecentActivitiesAsync(request.RecentCount);

            // Get counts
            var todayActivities = await _repository.GetActivitiesPagedAsync(
                from: startOfToday,
                to: now,
                page: 1,
                pageSize: 1
            );

            var weekActivities = await _repository.GetActivitiesPagedAsync(
                from: startOfWeek,
                to: now,
                page: 1,
                pageSize: 1
            );

            var monthActivities = await _repository.GetActivitiesPagedAsync(
                from: startOfMonth,
                to: now,
                page: 1,
                pageSize: 1
            );

            var activityTypeCounts = await _repository.GetActivityCountByTypeAsync(startOfToday, now);

            return new ActivitySummaryDTO
            {
                TotalToday = (int)todayActivities.TotalCount,
                TotalThisWeek = (int)weekActivities.TotalCount,
                TotalThisMonth = (int)monthActivities.TotalCount,
                RecentActivities = recentActivities.Select(MapToDTO).ToList(),
                ActivityTypeCount = activityTypeCounts.ToDictionary(
                    kvp => kvp.Key.ToString(),
                    kvp => kvp.Value
                )
            };
        }

        private static UserActivityDTO MapToDTO(UserActivityLog activity)
        {
            return new UserActivityDTO
            {
                ActivityId = activity.Id,
                UserId = activity.UserId,
                Username = activity.Username,
                ActivityType = activity.ActivityType.ToString(),
                EntityType = activity.EntityType,
                EntityId = activity.EntityId,
                Action = activity.Action,
                Metadata = activity.Metadata,
                Timestamp = activity.Timestamp,
                IPAddress = activity.IPAddress,
                IsSuccess = activity.IsSuccess,
                ErrorMessage = activity.ErrorMessage,
                DurationMs = activity.DurationMs,
                Module = activity.Module
            };
        }
    }

    /// <summary>
    /// Handler for GetActivityWidgetDataQuery (Lightweight Activity Widget)
    /// Returns only essential fields for activity feed widgets
    /// </summary>
    public class GetActivityWidgetDataQueryHandler : IRequestHandler<GetActivityWidgetDataQuery, List<ActivityWidgetDTO>>
    {
        private readonly IUserActivityRepository _repository;

        public GetActivityWidgetDataQueryHandler(IUserActivityRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ActivityWidgetDTO>> Handle(GetActivityWidgetDataQuery request, CancellationToken cancellationToken)
        {
            var activities = await _repository.GetRecentActivitiesAsync(request.Count);

            return activities.Select(activity => new ActivityWidgetDTO
            {
                DisplayMessage = activity.DisplayMessage,
                Timestamp = activity.Timestamp,
                Action = activity.Action
            }).ToList();
        }
    }
}
