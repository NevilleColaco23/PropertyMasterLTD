using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Dependencies.Services;
using MyWarehouse.Application.Common.Audit;
using MyWarehouse.Domain.Common.Menus;

namespace MyWarehouse.Application.Common.MenuPermissions
{
    public class CreateMenuPermissionCommand : IRequest<int>
    {
        public int UserId { get; init; }
        public int MenuId { get; init; }
        public string AccessLevel { get; init; } = "read";
        public bool IsActive { get; init; } = true;
        public DateTime From { get; init; }
        public DateTime To { get; init; }
    }

    public class CreateMenuPermissionCommandHandler : IRequestHandler<CreateMenuPermissionCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditService _auditService;

        public CreateMenuPermissionCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _auditService = auditService;
        }

        public async Task<int> Handle(CreateMenuPermissionCommand request, CancellationToken cancellationToken)
        {
            var userId = int.TryParse(_currentUserService.UserId, out var parsedUserId) ? parsedUserId : request.UserId;

            var menuPermission = new MenusPermissions
            {
                UserId = request.UserId,
                MenuID = request.MenuId,
                AccessLevel = request.AccessLevel,
                IsActive = request.IsActive,
                From = request.From,
                To = request.To,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _unitOfWork.MenuPermissions.Add(menuPermission, cancellationToken);
            await _unitOfWork.SaveChanges();

            // ✅ Log the create operation to audit trail
            await _auditService.LogCreate(
                "MenuPermissions",
                menuPermission.Id,
                menuPermission,
                userId
            );

            return menuPermission.Id;
        }
    }
}
