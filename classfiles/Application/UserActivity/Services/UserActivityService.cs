using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Domain.UserActivity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyWarehouse.Application.UserActivity.Services
{
    /// <summary>
    /// Service for logging user activities
    /// </summary>
    public class UserActivityService
    {
        private readonly IUserActivityRepository _repository;

        public UserActivityService(IUserActivityRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Logs a user activity
        /// </summary>
        public async Task<int> LogActivityAsync(
            int userId,
            string username,
            ActivityType activityType,
            string action,
            string description,
            string? entityType = null,
            int? entityId = null,
            Dictionary<string, object>? metadata = null,
            bool isSuccess = true,
            string? errorMessage = null,
            long? durationMs = null,
            string? module = null,
            int? propertyId = null,
            string? ipAddress = null,
            string? userAgent = null,
            string? sessionId = null,
            string? traceId = null,
            string? displayMessage = null)
        {
            var activity = new UserActivityLog
            {
                UserId = userId,
                Username = username,
                ActivityType = activityType,
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                Description = description,
                Metadata = metadata,
                DisplayMessage = displayMessage,
                Timestamp = DateTime.UtcNow,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                SessionId = sessionId,
                TraceId = traceId,
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage,
                DurationMs = durationMs,
                Module = module,
                PropertyId = propertyId
            };

            return await _repository.LogActivityAsync(activity);
        }

        /// <summary>
        /// Logs a successful CRUD operation
        /// </summary>
        public async Task LogCrudOperationAsync(
            int userId,
            string username,
            ActivityType activityType,
            string entityType,
            int entityId,
            string entityName,
            Dictionary<string, object>? metadata = null)
        {
            string action = activityType switch
            {
                ActivityType.Create => $"Created {entityType.ToLower()}",
                ActivityType.Update => $"Updated {entityType.ToLower()}",
                ActivityType.Delete => $"Deleted {entityType.ToLower()}",
                ActivityType.View => $"Viewed {entityType.ToLower()}",
                _ => $"{activityType} {entityType.ToLower()}"
            };

            string description = $"{action}: {entityName} (ID: {entityId})";

            await LogActivityAsync(
                userId,
                username,
                activityType,
                action,
                description,
                entityType,
                entityId,
                metadata,
                isSuccess: true
            );
        }

        /// <summary>
        /// Logs a page view
        /// </summary>
        public async Task LogPageViewAsync(
            int userId,
            string username,
            string pageName,
            string pageUrl)
        {
            var metadata = new Dictionary<string, object>
            {
                { "PageUrl", pageUrl }
            };

            await LogActivityAsync(
                userId,
                username,
                ActivityType.PageView,
                $"Viewed {pageName}",
                $"User accessed {pageName} page",
                metadata: metadata
            );
        }

        /// <summary>
        /// Logs a login event
        /// </summary>
        public async Task LogLoginAsync(
            int userId,
            string username,
            bool isSuccess,
            string? errorMessage = null)
        {
            await LogActivityAsync(
                userId,
                username,
                ActivityType.Login,
                "User Login",
                isSuccess ? $"{username} logged in successfully" : $"{username} failed to login",
                isSuccess: isSuccess,
                errorMessage: errorMessage
            );
        }

        /// <summary>
        /// Logs a logout event
        /// </summary>
        public async Task LogLogoutAsync(int userId, string username)
        {
            await LogActivityAsync(
                userId,
                username,
                ActivityType.Logout,
                "User Logout",
                $"{username} logged out"
            );
        }

        /// <summary>
        /// Logs an export operation
        /// </summary>
        public async Task LogExportAsync(
            int userId,
            string username,
            string exportType,
            int recordCount,
            string? entityType = null)
        {
            var metadata = new Dictionary<string, object>
            {
                { "ExportType", exportType },
                { "RecordCount", recordCount }
            };

            await LogActivityAsync(
                userId,
                username,
                ActivityType.Export,
                $"Exported {exportType}",
                $"User exported {recordCount} {entityType ?? "records"} to {exportType}",
                entityType: entityType,
                metadata: metadata
            );
        }

        /// <summary>
        /// Logs a search operation
        /// </summary>
        public async Task LogSearchAsync(
            int userId,
            string username,
            string searchTerm,
            string? entityType = null,
            int? resultCount = null)
        {
            var metadata = new Dictionary<string, object>
            {
                { "SearchTerm", searchTerm }
            };

            if (resultCount.HasValue)
                metadata.Add("ResultCount", resultCount.Value);

            await LogActivityAsync(
                userId,
                username,
                ActivityType.Search,
                "Search",
                $"User searched for: {searchTerm}" + (resultCount.HasValue ? $" ({resultCount} results)" : ""),
                entityType: entityType,
                metadata: metadata
            );
        }

        /// <summary>
        /// Logs a dashboard operation
        /// </summary>
        public async Task LogDashboardOperationAsync(
            int userId,
            string username,
            ActivityType activityType,
            string dashboardName,
            int? dashboardId = null,
            Dictionary<string, object>? metadata = null)
        {
            string action = activityType switch
            {
                ActivityType.DashboardCreate => "Created dashboard",
                ActivityType.DashboardUpdate => "Updated dashboard",
                ActivityType.DashboardDelete => "Deleted dashboard",
                ActivityType.DashboardView => "Viewed dashboard",
                ActivityType.WidgetAdd => "Added widget",
                ActivityType.WidgetRemove => "Removed widget",
                ActivityType.WidgetConfigure => "Configured widget",
                _ => $"{activityType} dashboard"
            };

            await LogActivityAsync(
                userId,
                username,
                activityType,
                action,
                $"{action}: {dashboardName}",
                entityType: "Dashboard",
                entityId: dashboardId,
                metadata: metadata
            );
        }
    }
}
