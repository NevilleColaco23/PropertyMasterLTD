using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;

namespace MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories
{
    public interface IMenuRepository : IRepository<MyWarehouse.Domain.Common.Menus.Menus, int>
    {
    }
}