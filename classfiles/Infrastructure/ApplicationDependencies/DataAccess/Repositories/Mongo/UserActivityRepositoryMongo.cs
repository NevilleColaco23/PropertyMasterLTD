using MongoDB.Driver;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Domain.UserActivity;
using MyWarehouse.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Mongo
{
    /// <summary>
    /// MongoDB implementation of user activity repository
    /// </summary>
    public class UserActivityRepositoryMongo : IUserActivityRepository
    {
        private readonly IMongoCollection<UserActivityLog> _collection;
        private readonly ICounterService _counterService;

        public UserActivityRepositoryMongo(IMongoDatabase database, ICounterService counterService)
        {
            _collection = database.GetCollection<UserActivityLog>(Application.MongoCollections.UserActivityLogsCollection);
            _counterService = counterService;

            // Create indexes for performance
            CreateIndexes();
        }

        private void CreateIndexes()
        {
            // Index for recent activities query
            var timestampIndex = Builders<UserActivityLog>.IndexKeys.Descending(x => x.Timestamp);
            _collection.Indexes.CreateOne(new CreateIndexModel<UserActivityLog>(timestampIndex));

            // Index for user activities
            var userTimestampIndex = Builders<UserActivityLog>.IndexKeys
                .Ascending(x => x.UserId)
                .Descending(x => x.Timestamp);
            _collection.Indexes.CreateOne(new CreateIndexModel<UserActivityLog>(userTimestampIndex));

            // Index for entity activities
            var entityIndex = Builders<UserActivityLog>.IndexKeys
                .Ascending(x => x.EntityType)
                .Ascending(x => x.EntityId)
                .Descending(x => x.Timestamp);
            _collection.Indexes.CreateOne(new CreateIndexModel<UserActivityLog>(entityIndex));

            // Index for activity type filtering
            var activityTypeIndex = Builders<UserActivityLog>.IndexKeys
                .Ascending(x => x.ActivityType)
                .Descending(x => x.Timestamp);
            _collection.Indexes.CreateOne(new CreateIndexModel<UserActivityLog>(activityTypeIndex));
        }

        public async Task<int> LogActivityAsync(UserActivityLog activity)
        {
            // Generate ID using counter service
            activity.Id = await _counterService.GetNextSequenceValue(Application.MongoCollections.UserActivityLogsCollection);

            await _collection.InsertOneAsync(activity);
            return activity.Id;
        }

        public async Task<List<UserActivityLog>> GetRecentActivitiesAsync(int count = 50, int skip = 0)
        {
            return await _collection
                .Find(_ => true)
                .SortByDescending(x => x.Timestamp)
                .Skip(skip)
                .Limit(count)
                .ToListAsync();
        }

        public async Task<List<UserActivityLog>> GetUserActivitiesAsync(
            int userId,
            DateTime? from = null,
            DateTime? to = null,
            int? limit = null)
        {
            var filterBuilder = Builders<UserActivityLog>.Filter;
            var filter = filterBuilder.Eq(x => x.UserId, userId);

            if (from.HasValue)
                filter &= filterBuilder.Gte(x => x.Timestamp, from.Value);

            if (to.HasValue)
                filter &= filterBuilder.Lte(x => x.Timestamp, to.Value);

            var query = _collection
                .Find(filter)
                .SortByDescending(x => x.Timestamp);

            if (limit.HasValue)
                return await query.Limit(limit.Value).ToListAsync();

            return await query.ToListAsync();
        }

        public async Task<List<UserActivityLog>> GetActivitiesByTypeAsync(
            ActivityType activityType,
            DateTime? from = null,
            DateTime? to = null,
            int? limit = null)
        {
            var filterBuilder = Builders<UserActivityLog>.Filter;
            var filter = filterBuilder.Eq(x => x.ActivityType, activityType);

            if (from.HasValue)
                filter &= filterBuilder.Gte(x => x.Timestamp, from.Value);

            if (to.HasValue)
                filter &= filterBuilder.Lte(x => x.Timestamp, to.Value);

            var query = _collection
                .Find(filter)
                .SortByDescending(x => x.Timestamp);

            if (limit.HasValue)
                return await query.Limit(limit.Value).ToListAsync();

            return await query.ToListAsync();
        }

        public async Task<List<UserActivityLog>> GetEntityActivitiesAsync(
            string entityType,
            int entityId,
            DateTime? from = null,
            DateTime? to = null)
        {
            var filterBuilder = Builders<UserActivityLog>.Filter;
            var filter = filterBuilder.Eq(x => x.EntityType, entityType) &
                        filterBuilder.Eq(x => x.EntityId, entityId);

            if (from.HasValue)
                filter &= filterBuilder.Gte(x => x.Timestamp, from.Value);

            if (to.HasValue)
                filter &= filterBuilder.Lte(x => x.Timestamp, to.Value);

            return await _collection
                .Find(filter)
                .SortByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        public async Task<Dictionary<string, object>> GetActivityStatisticsAsync(
            DateTime? from = null,
            DateTime? to = null)
        {
            var filterBuilder = Builders<UserActivityLog>.Filter;
            var filter = filterBuilder.Empty;

            if (from.HasValue)
                filter &= filterBuilder.Gte(x => x.Timestamp, from.Value);

            if (to.HasValue)
                filter &= filterBuilder.Lte(x => x.Timestamp, to.Value);

            var totalActivities = await _collection.CountDocumentsAsync(filter);
            var uniqueUsers = await _collection.DistinctAsync(x => x.UserId, filter);
            var uniqueUserCount = (await uniqueUsers.ToListAsync()).Count;

            var activities = await _collection.Find(filter).ToListAsync();
            var activityTypes = activities
                .GroupBy(x => x.ActivityType)
                .ToDictionary(g => g.Key.ToString(), g => (object)g.Count());

            return new Dictionary<string, object>
            {
                { "TotalActivities", totalActivities },
                { "UniqueUsers", uniqueUserCount },
                { "ActivityTypes", activityTypes },
                { "From", from?.ToString("yyyy-MM-dd HH:mm:ss") ?? "All time" },
                { "To", to?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Now" }
            };
        }

        public async Task<List<(string Username, int ActivityCount)>> GetMostActiveUsersAsync(
            int count = 10,
            DateTime? from = null,
            DateTime? to = null)
        {
            var filterBuilder = Builders<UserActivityLog>.Filter;
            var filter = filterBuilder.Empty;

            if (from.HasValue)
                filter &= filterBuilder.Gte(x => x.Timestamp, from.Value);

            if (to.HasValue)
                filter &= filterBuilder.Lte(x => x.Timestamp, to.Value);

            var activities = await _collection.Find(filter).ToListAsync();
            
            return activities
                .GroupBy(x => x.Username)
                .Select(g => (g.Key, g.Count()))
                .OrderByDescending(x => x.Item2)
                .Take(count)
                .ToList();
        }

        public async Task<Dictionary<ActivityType, int>> GetActivityCountByTypeAsync(
            DateTime? from = null,
            DateTime? to = null)
        {
            var filterBuilder = Builders<UserActivityLog>.Filter;
            var filter = filterBuilder.Empty;

            if (from.HasValue)
                filter &= filterBuilder.Gte(x => x.Timestamp, from.Value);

            if (to.HasValue)
                filter &= filterBuilder.Lte(x => x.Timestamp, to.Value);

            var activities = await _collection.Find(filter).ToListAsync();

            return activities
                .GroupBy(x => x.ActivityType)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<long> DeleteOldActivitiesAsync(DateTime olderThan)
        {
            var filter = Builders<UserActivityLog>.Filter.Lt(x => x.Timestamp, olderThan);
            var result = await _collection.DeleteManyAsync(filter);
            return result.DeletedCount;
        }

        public async Task<(List<UserActivityLog> Activities, long TotalCount)> GetActivitiesPagedAsync(
            int? userId = null,
            ActivityType? activityType = null,
            string? entityType = null,
            DateTime? from = null,
            DateTime? to = null,
            int page = 1,
            int pageSize = 50,
            string sortBy = "Timestamp",
            bool sortDescending = true)
        {
            var filterBuilder = Builders<UserActivityLog>.Filter;
            var filter = filterBuilder.Empty;

            if (userId.HasValue)
                filter &= filterBuilder.Eq(x => x.UserId, userId.Value);

            if (activityType.HasValue)
                filter &= filterBuilder.Eq(x => x.ActivityType, activityType.Value);

            if (!string.IsNullOrEmpty(entityType))
                filter &= filterBuilder.Eq(x => x.EntityType, entityType);

            if (from.HasValue)
                filter &= filterBuilder.Gte(x => x.Timestamp, from.Value);

            if (to.HasValue)
                filter &= filterBuilder.Lte(x => x.Timestamp, to.Value);

            var totalCount = await _collection.CountDocumentsAsync(filter);

            var query = _collection.Find(filter);

            // Apply sorting
            query = sortDescending
                ? query.SortByDescending(x => x.Timestamp)
                : query.SortBy(x => x.Timestamp);

            // Apply pagination
            var skip = (page - 1) * pageSize;
            var activities = await query.Skip(skip).Limit(pageSize).ToListAsync();

            return (activities, totalCount);
        }

        /// <summary>
        /// Finds activities matching the provided filter (for analytics queries)
        /// </summary>
        public async Task<IEnumerable<UserActivityLog>> FindAsync(FilterDefinition<UserActivityLog> filter)
        {
            var cursor = await _collection.FindAsync(filter);
            return await cursor.ToListAsync();
        }

        /// <summary>
        /// Counts activities matching the provided filter
        /// </summary>
        public async Task<long> CountAsync(FilterDefinition<UserActivityLog> filter)
        {
            return await _collection.CountDocumentsAsync(filter);
        }
    }
}
