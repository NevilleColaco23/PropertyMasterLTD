using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;

namespace MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories
{
    public interface IPropertyAuditRepository : IRepository<Domain.Property.PropertyAudit, int>
    {
    }
}
