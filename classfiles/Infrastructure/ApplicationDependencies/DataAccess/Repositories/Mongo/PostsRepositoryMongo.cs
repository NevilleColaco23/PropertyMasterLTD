using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Common;
using MongoDB.Driver;
using MyWarehouse.Application;
using MyWarehouse.Domain.Posts;
using MyWarehouse.Application.Services;

namespace MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Mongo;

public class PostsRepositoryMongo : RepositoryBaseMongo<Post, int>, IPostsRepository
{
    public PostsRepositoryMongo(IMongoDatabase database, IMapper mapper, ICounterService counterService)
        : base(database, mapper, MongoCollections.PostsCollection, counterService)
    {
    }

    public async Task<List<Post>> GetRecentAsync(int limit)
    {
        return await Collection
            .Find(FilterDefinition<Post>.Empty)
            .SortByDescending(p => p.CreatedAt)
            .Limit(limit)
            .ToListAsync();
    }
}
