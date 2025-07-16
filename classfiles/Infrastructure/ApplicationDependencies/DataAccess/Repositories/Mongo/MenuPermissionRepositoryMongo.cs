using MongoDB.Driver;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Application.Services;
using MyWarehouse.Application;
using MyWarehouse.Domain.Common.Menus;
using MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Common;

namespace MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Mongo
{
    internal class MenuPermissionRepositoryMongo : RepositoryBaseMongo<MenusPermissions, int>, IMenuPermissionRepository
    {
        public MenuPermissionRepositoryMongo(IMongoDatabase database, IMapper mapper, ICounterService counterService)
            : base(database, mapper, MongoCollections.MenuPermissionsCollection, counterService)
        {
        }
    }
}
