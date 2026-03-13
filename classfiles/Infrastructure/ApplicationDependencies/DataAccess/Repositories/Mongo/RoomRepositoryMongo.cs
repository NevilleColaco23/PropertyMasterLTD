using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Common;
using MongoDB.Driver;
using MyWarehouse.Application;
using MyWarehouse.Domain.Property;
using MyWarehouse.Application.Services;

namespace MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Mongo;

public class RoomRepositoryMongo : RepositoryBaseMongo<Room, int>, IRoomRepository
{
    public RoomRepositoryMongo(IMongoDatabase database, IMapper mapper, ICounterService counterService)
        : base(database, mapper, MongoCollections.RoomCollection, counterService)
    {
    }

    public async Task<List<Room>> GetRoomsByPropertyIdAsync(int propertyId)
    {
        var filter = Builders<Room>.Filter.Eq(r => r.PropertyId, propertyId);
        return await Collection.Find(filter).ToListAsync();
    }

    public async Task DeleteRoomsByPropertyIdAsync(int propertyId)
    {
        var filter = Builders<Room>.Filter.Eq(r => r.PropertyId, propertyId);
        await Collection.DeleteManyAsync(filter);
    }
}
