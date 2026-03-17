using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;

namespace MyWarehouse.Application.Common.Dependencies.DataAccess;

public interface IUnitOfWork : IDisposable
{
    public IPartnerRepository Partners { get; }
    public IProductRepository Products { get; }
    public ITransactionRepository Transactions { get; }
    public IPropertyRepository? Properties { get; }
    public IRoomRepository? Rooms { get; }
    public IPropertyAuditRepository? PropertyAudits { get; }
    public IRoomAuditRepository? RoomAudits { get; }
    public IUsersRepository? Users { get; }
    public IMenuRepository? Menus { get; }
    public IMenuPermissionRepository? MenuPermissions { get; }
    public IBookingsRepository? Bookings { get; }
    public ISystemMessagesRepository? SystemMessages { get; }
    public IEmailOutboxRepository? EmailOutbox { get; }
    public IAuditLogRepository? AuditLogs { get; }

    bool HasActiveTransaction { get; }

    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    public Task SaveChanges();
}
