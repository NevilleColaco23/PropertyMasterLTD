using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;

namespace MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories
{
    public interface IRoomRepository : IRepository<Domain.Property.Room, int>
    {
        Task<List<Domain.Property.Room>> GetRoomsByPropertyIdAsync(int propertyId);
        Task DeleteRoomsByPropertyIdAsync(int propertyId);
    }
}
