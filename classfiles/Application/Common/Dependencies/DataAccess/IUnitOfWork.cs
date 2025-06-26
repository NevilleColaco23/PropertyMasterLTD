using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;

namespace MyWarehouse.Application.Common.Dependencies.DataAccess;

public interface IUnitOfWork : IDisposable
{
    public IPartnerRepository Partners { get; }
    public IProductRepository Products { get; }
    public ITransactionRepository Transactions { get; }
    public IPropertyRepository? Properties { get; }
    public IUsersRepository? Users { get; }
    public IAccessLogRepository? AccessLogs { get; }
    public IMenuRepository? Menus { get; }

    bool HasActiveTransaction { get; }

    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    public Task SaveChanges();
}
