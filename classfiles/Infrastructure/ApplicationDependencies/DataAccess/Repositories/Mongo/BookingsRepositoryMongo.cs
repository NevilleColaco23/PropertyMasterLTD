using MongoDB.Driver;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Application.Services;
using MyWarehouse.Application;
using MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Common;
using MyWarehouse.Domain.Bookings;
using MongoDB.Bson;

namespace MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Mongo
{
    public class BookingsRepositoryMongo : RepositoryBaseMongo<Bookings, long>, IBookingsRepository
    {
        public BookingsRepositoryMongo(IMongoDatabase database, IMapper mapper, ICounterService counterService)
            : base(database, mapper, MongoCollections.BookingsCollection, counterService)
        {
        }
    }
}