using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;

namespace MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories
{
    public interface IPropertyRepository : IRepository<Domain.Property.Property, int>
    {
    }
}