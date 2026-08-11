using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Common;
using MongoDB.Driver;
using MyWarehouse.Application;
using MyWarehouse.Domain.SystemSettings;
using MyWarehouse.Application.Services;

namespace MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Mongo;

public class FeedSettingsRepositoryMongo : RepositoryBaseMongo<FeedSettings, int>, IFeedSettingsRepository
{
    public FeedSettingsRepositoryMongo(IMongoDatabase database, IMapper mapper, ICounterService counterService)
        : base(database, mapper, MongoCollections.SystemSettingsCollection, counterService)
    {
    }

    public async Task<FeedSettings?> GetByUserIdAsync(int userId)
    {
        var filter = Builders<FeedSettings>.Filter.And(
            Builders<FeedSettings>.Filter.Eq(x => x.SettingType, SettingTypes.TeamFeed),
            Builders<FeedSettings>.Filter.Eq(x => x.UserId, userId));

        return await Collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<FeedSettings> UpsertAsync(int userId, int maxPostsToShow)
    {
        var existing = await GetByUserIdAsync(userId);

        if (existing == null)
        {
            var created = new FeedSettings(userId, maxPostsToShow);
            return await Add(created);
        }

        existing.MaxPostsToShow = maxPostsToShow;
        existing.UpdatedAt = DateTime.UtcNow;
        return await Update(existing);
    }
}
