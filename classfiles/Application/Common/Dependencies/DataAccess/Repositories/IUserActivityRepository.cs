using MongoDB.Driver;
using MyWarehouse.Domain.UserActivity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories
{
    /// <summary>
    /// Repository interface for user activity logging operations
    /// </summary>
    public interface IUserActivityRepository
    {
        /// <summary>
        /// Logs a user activity to the database
        /// </summary>
        Task<int> LogActivityAsync(UserActivityLog activity);

        /// <summary>
        /// Gets recent activities across all users
        /// </summary>
        /// <param name="count">Number of activities to retrieve</param>
        /// <param name="skip">Number of activities to skip (for pagination)</param>
        Task<List<UserActivityLog>> GetRecentActivitiesAsync(int count = 50, int skip = 0);

        /// <summary>
        /// Gets activities for a specific user
        /// </summary>
        Task<List<UserActivityLog>> GetUserActivitiesAsync(
            int userId, 
            DateTime? from = null, 
            DateTime? to = null,
            int? limit = null);

        /// <summary>
        /// Gets activities by type
        /// </summary>
        Task<List<UserActivityLog>> GetActivitiesByTypeAsync(
            ActivityType activityType,
            DateTime? from = null,
            DateTime? to = null,
            int? limit = null);

        /// <summary>
        /// Gets activities for a specific entity
        /// </summary>
        Task<List<UserActivityLog>> GetEntityActivitiesAsync(
            string entityType,
            int entityId,
            DateTime? from = null,
            DateTime? to = null);

        /// <summary>
        /// Gets activity statistics
        /// </summary>
        Task<Dictionary<string, object>> GetActivityStatisticsAsync(
            DateTime? from = null,
            DateTime? to = null);

        /// <summary>
        /// Gets most active users
        /// </summary>
        Task<List<(string Username, int ActivityCount)>> GetMostActiveUsersAsync(
            int count = 10,
            DateTime? from = null,
            DateTime? to = null);

        /// <summary>
        /// Gets activity count by type
        /// </summary>
        Task<Dictionary<ActivityType, int>> GetActivityCountByTypeAsync(
            DateTime? from = null,
            DateTime? to = null);

        /// <summary>
        /// Deletes old activity logs (for maintenance)
        /// </summary>
        Task<long> DeleteOldActivitiesAsync(DateTime olderThan);

        /// <summary>
        /// Gets activities with filtering and pagination
        /// </summary>
        Task<(List<UserActivityLog> Activities, long TotalCount)> GetActivitiesPagedAsync(
            int? userId = null,
            ActivityType? activityType = null,
            string? entityType = null,
            DateTime? from = null,
            DateTime? to = null,
            int page = 1,
            int pageSize = 50,
            string sortBy = "Timestamp",
            bool sortDescending = true);

        /// <summary>
        /// Finds activities matching the provided filter (for analytics queries)
        /// </summary>
        Task<IEnumerable<UserActivityLog>> FindAsync(FilterDefinition<UserActivityLog> filter);

        /// <summary>
        /// Counts activities matching the provided filter
        /// </summary>
        Task<long> CountAsync(FilterDefinition<UserActivityLog> filter);
    }
}
