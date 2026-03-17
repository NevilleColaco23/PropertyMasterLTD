using MyWarehouse.Domain.UserActivity;
using System;
using System.Collections.Generic;

namespace MyWarehouse.Application.UserActivity.DTOs
{
    /// <summary>
    /// DTO for user activity display
    /// </summary>
    public class UserActivityDTO
    {
        public int ActivityId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string ActivityType { get; set; }
        public string EntityType { get; set; }
        public int? EntityId { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Human-readable activity message for reports and widgets
        /// Example: "John Doe viewed All Dashboards while working on Sunset Villa property"
        /// </summary>
        public string DisplayMessage { get; set; }

        public Dictionary<string, object>? Metadata { get; set; }
        public DateTime Timestamp { get; set; }
        public string IPAddress { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public long? DurationMs { get; set; }
        public string Module { get; set; }

        /// <summary>
        /// Time ago formatted string (e.g., "5 minutes ago")
        /// </summary>
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.UtcNow - Timestamp;
                
                if (timeSpan.TotalMinutes < 1)
                    return "just now";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes != 1 ? "s" : "")} ago";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours != 1 ? "s" : "")} ago";
                if (timeSpan.TotalDays < 30)
                    return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays != 1 ? "s" : "")} ago";
                if (timeSpan.TotalDays < 365)
                    return $"{(int)(timeSpan.TotalDays / 30)} month{((int)(timeSpan.TotalDays / 30) != 1 ? "s" : "")} ago";
                
                return $"{(int)(timeSpan.TotalDays / 365)} year{((int)(timeSpan.TotalDays / 365) != 1 ? "s" : "")} ago";
            }
        }
    }

    /// <summary>
    /// DTO for paginated activity results
    /// </summary>
    public class PagedActivitiesDTO
    {
        public List<UserActivityDTO> Activities { get; set; } = new();
        public long TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }

    /// <summary>
    /// DTO for activity statistics
    /// </summary>
    public class ActivityStatisticsDTO
    {
        public long TotalActivities { get; set; }
        public int UniqueUsers { get; set; }
        public Dictionary<string, int> ActivityTypeBreakdown { get; set; } = new();
        public List<TopUserActivityDTO> TopUsers { get; set; } = new();
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Period { get; set; }
    }

    /// <summary>
    /// DTO for top active users
    /// </summary>
    public class TopUserActivityDTO
    {
        public string Username { get; set; }
        public int ActivityCount { get; set; }
        public string MostCommonActivity { get; set; }
    }

    /// <summary>
    /// DTO for activity summary (for dashboard widget)
    /// </summary>
    public class ActivitySummaryDTO
    {
        public int TotalToday { get; set; }
        public int TotalThisWeek { get; set; }
        public int TotalThisMonth { get; set; }
        public List<UserActivityDTO> RecentActivities { get; set; } = new();
        public Dictionary<string, int> ActivityTypeCount { get; set; } = new();
    }

    /// <summary>
    /// Lightweight DTO for activity widget (displays only essential fields)
    /// Used for dashboard widgets showing recent activity feed
    /// </summary>
    public class ActivityWidgetDTO
    {
        /// <summary>
        /// Human-readable activity message
        /// Example: "John Doe viewed All Dashboards while working on Sunset Villa property"
        /// </summary>
        public string DisplayMessage { get; set; }

        /// <summary>
        /// When the activity occurred
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Action performed (e.g., "View Property", "Create Booking", "Update Room")
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Time ago formatted string (e.g., "5 minutes ago")
        /// </summary>
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.UtcNow - Timestamp;

                if (timeSpan.TotalMinutes < 1)
                    return "just now";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes != 1 ? "s" : "")} ago";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours != 1 ? "s" : "")} ago";
                if (timeSpan.TotalDays < 30)
                    return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays != 1 ? "s" : "")} ago";
                if (timeSpan.TotalDays < 365)
                    return $"{(int)(timeSpan.TotalDays / 30)} month{((int)(timeSpan.TotalDays / 30) != 1 ? "s" : "")} ago";

                return $"{(int)(timeSpan.TotalDays / 365)} year{((int)(timeSpan.TotalDays / 365) != 1 ? "s" : "")} ago";
            }
        }
    }
}
