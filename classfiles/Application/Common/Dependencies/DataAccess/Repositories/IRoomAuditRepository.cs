using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;

namespace MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories
{
    public interface IRoomAuditRepository : IRepository<Domain.Property.RoomAudit, int>
    {
        Task<List<Domain.Property.RoomAudit>> GetAuditedRoomsByPropertyIdAsync(int propertyId);
    }
}
