using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Dependencies.Services;
using MyWarehouse.Application.Common.Audit;

namespace MyWarehouse.Application.Common.MenuPermissions
{
    public class DeleteMenuPermissionCommand : IRequest<Unit>
    {
        public int Id { get; init; }
    }

    public class DeleteMenuPermissionCommandHandler : IRequestHandler<DeleteMenuPermissionCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditService _auditService;

        public DeleteMenuPermissionCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _auditService = auditService;
        }

        public async Task<Unit> Handle(DeleteMenuPermissionCommand request, CancellationToken cancellationToken)
        {
            var permission = await _unitOfWork.MenuPermissions.GetByIdAsync(request.Id);
            if (permission != null)
            {
                var currentUserId = int.TryParse(_currentUserService.UserId, out var userId) ? userId : 0;

                _unitOfWork.MenuPermissions.Remove(permission);
                await _unitOfWork.SaveChanges();

                // Log the delete operation to audit trail
                await _auditService.LogDelete(
                    "MenuPermissions",
                    request.Id,
                    permission,
                    currentUserId
                );
            }

            return Unit.Value;
        }
    }
}
