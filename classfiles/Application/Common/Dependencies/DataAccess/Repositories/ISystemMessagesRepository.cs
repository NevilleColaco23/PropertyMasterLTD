using MongoDB.Bson;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Domain.System_Related.System_Messages;

namespace MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories
{
    public interface ISystemMessagesRepository : IRepository<MyWarehouse.Domain.System_Related.System_Messages.SystemMessages, ObjectId>
    {
    }
}
