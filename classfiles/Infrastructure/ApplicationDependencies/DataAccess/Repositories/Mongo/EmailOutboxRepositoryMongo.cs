using MongoDB.Driver;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Application.Services;
using MyWarehouse.Application;
using MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Common;
using MyWarehouse.Domain.System_Related.EmailOutbox;

namespace MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Mongo
{
    public class EmailOutboxRepositoryMongo : RepositoryBaseMongo<EmailOutbox, int>, IEmailOutboxRepository
    {
        public EmailOutboxRepositoryMongo(IMongoDatabase database, IMapper mapper, ICounterService counterService)
            : base(database, mapper, MongoCollections.EmailOutboxCollection, counterService)
        {
        }
    }
}
