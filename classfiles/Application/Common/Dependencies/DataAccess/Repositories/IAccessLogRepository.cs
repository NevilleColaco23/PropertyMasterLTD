using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Domain.AccessLog;

namespace MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories
{
    public interface IAccessLogRepository : IRepository<AccessLog, int>
    {
    }
}
