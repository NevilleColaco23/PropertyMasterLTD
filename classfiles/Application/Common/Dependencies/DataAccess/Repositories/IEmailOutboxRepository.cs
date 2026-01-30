using MongoDB.Bson;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;

namespace MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories
{
    public interface IEmailOutboxRepository : IRepository<Domain.System_Related.EmailOutbox.EmailOutbox, int>
    {
    }
}
