using MongoDB.Driver;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Application.Services;
using MyWarehouse.Application;
using MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Common;
using MyWarehouse.Domain.Users;

namespace MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Mongo
{
    public class UsersRepositoryMongo : RepositoryBaseMongo<Users, int>, IUsersRepository
    {
        public UsersRepositoryMongo(IMongoDatabase database, IMapper mapper, ICounterService counterService)
            : base(database, mapper, MongoCollections.UsersCollection, counterService)
        {
        }
    }
}