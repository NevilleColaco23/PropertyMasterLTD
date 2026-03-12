using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Domain.Common.Audit;

namespace MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories
{
    public interface IAuditLogRepository : IRepository<AuditLog, int>
    {
    }
}
