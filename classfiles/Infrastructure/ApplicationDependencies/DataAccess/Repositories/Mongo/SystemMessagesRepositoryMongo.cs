using MongoDB.Driver;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Application.Services;
using MyWarehouse.Application;
using MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Common;
using MyWarehouse.Domain.System_Related.System_Messages;
using MongoDB.Bson;

namespace MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Mongo
{
    public class SystemMessagesRepositoryMongo : RepositoryBaseMongo<SystemMessages, ObjectId>, ISystemMessagesRepository
    {
        public SystemMessagesRepositoryMongo(IMongoDatabase database, IMapper mapper, ICounterService counterService)
            : base(database, mapper, MongoCollections.SystemMessagesCollection, counterService)
        {
        }
    }
}
