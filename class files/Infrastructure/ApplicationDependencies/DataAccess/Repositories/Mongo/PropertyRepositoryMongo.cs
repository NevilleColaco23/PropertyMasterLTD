using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Common;
using MongoDB.Driver;
using MyWarehouse.Application;
using MyWarehouse.Domain.Property;
using MyWarehouse.Application.Services;

namespace MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Mongo;

public class PropertyRepositoryMongo : RepositoryBaseMongo<Property, int>, IPropertyRepository
{
    public PropertyRepositoryMongo(IMongoDatabase database, IMapper mapper, ICounterService counterService)
        : base(database, mapper, MongoCollections.PropertyCollection, counterService)
    {
    }
}