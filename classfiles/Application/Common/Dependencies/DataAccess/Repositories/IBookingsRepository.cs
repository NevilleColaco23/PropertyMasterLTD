using MongoDB.Bson;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;

namespace MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories
{
    public interface IBookingsRepository : IRepository<MyWarehouse.Domain.Bookings.Bookings, int>
    {
    }
}
