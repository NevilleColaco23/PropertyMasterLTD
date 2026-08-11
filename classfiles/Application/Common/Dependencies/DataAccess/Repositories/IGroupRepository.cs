using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Domain.Groups;

namespace MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories
{
    public interface IGroupRepository : IRepository<Group, int>
    {
        Task<List<Group>> GetAllAsync();
    }
}
