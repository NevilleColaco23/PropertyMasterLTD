using MongoDB.Driver;
using MyWarehouse.Application;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Application.Services;
using MyWarehouse.Domain.Common.Audit;
using MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Common;

namespace MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Mongo
{
    internal class AuditLogRepositoryMongo : RepositoryBaseMongo<AuditLog, int>, IAuditLogRepository
    {
        public AuditLogRepositoryMongo(IMongoDatabase database, IMapper mapper, ICounterService counterService)
            : base(database, mapper, MongoCollections.AuditLogsCollection, counterService)
        {
        }
    }
}
