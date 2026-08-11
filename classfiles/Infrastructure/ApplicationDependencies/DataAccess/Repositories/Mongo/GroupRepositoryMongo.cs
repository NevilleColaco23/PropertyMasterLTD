using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Common;
using MongoDB.Driver;
using MyWarehouse.Application;
using MyWarehouse.Domain.Groups;
using MyWarehouse.Application.Services;

namespace MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Mongo;

public class GroupRepositoryMongo : RepositoryBaseMongo<Group, int>, IGroupRepository
{
    public GroupRepositoryMongo(IMongoDatabase database, IMapper mapper, ICounterService counterService)
        : base(database, mapper, MongoCollections.GroupsCollection, counterService)
    {
    }

    public async Task<List<Group>> GetAllAsync()
    {
        return await Collection.Find(FilterDefinition<Group>.Empty).ToListAsync();
    }
}
