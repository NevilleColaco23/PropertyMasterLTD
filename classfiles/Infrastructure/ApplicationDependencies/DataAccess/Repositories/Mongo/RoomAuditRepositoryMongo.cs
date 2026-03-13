using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Common;
using MongoDB.Driver;
using MyWarehouse.Application;
using MyWarehouse.Domain.Property;
using MyWarehouse.Application.Services;

namespace MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Mongo;

public class RoomAuditRepositoryMongo : RepositoryBaseMongo<RoomAudit, int>, IRoomAuditRepository
{
    public RoomAuditRepositoryMongo(IMongoDatabase database, IMapper mapper, ICounterService counterService)
        : base(database, mapper, MongoCollections.RoomAuditCollection, counterService)
    {
    }

    public async Task<List<RoomAudit>> GetAuditedRoomsByPropertyIdAsync(int propertyId)
    {
        var filter = Builders<RoomAudit>.Filter.Eq(r => r.PropertyId, propertyId);
        return await Collection.Find(filter).ToListAsync();
    }
}
