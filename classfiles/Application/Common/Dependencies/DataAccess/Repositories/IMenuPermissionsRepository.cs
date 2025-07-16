using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;

namespace MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories
{
    public interface IMenuPermissionRepository : IRepository<MyWarehouse.Domain.Common.Menus.MenusPermissions, int>
    {
    }
}
