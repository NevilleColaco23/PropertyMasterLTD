using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Dependencies.Services;
using MyWarehouse.Application.Common.Audit;
using MyWarehouse.Domain.Common.Menus;

namespace MyWarehouse.Application.Common.MenuPermissions
{
    public class UpdateMenuPermissionCommand : IRequest<Unit>
    {
        public int Id { get; init; }
        public int UserId { get; init; }
        public int MenuId { get; init; }
        public string AccessLevel { get; init; } = "read";
        public bool IsActive { get; init; } = true;
        public DateTime From { get; init; }
        public DateTime To { get; init; }
    }

    public class UpdateMenuPermissionCommandHandler : IRequestHandler<UpdateMenuPermissionCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateMenuPermissionCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Unit> Handle(UpdateMenuPermissionCommand request, CancellationToken cancellationToken)
        {
            // Get current user ID
            var currentUserId = int.TryParse(_currentUserService.UserId, out var userId) ? userId : 0;

            var menuPermission = new MenusPermissions
            {
                Id = request.Id,
                UserId = request.UserId,
                MenuID = request.MenuId,
                AccessLevel = request.AccessLevel,
                IsActive = request.IsActive,
                From = request.From,
                To = request.To,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = currentUserId
            };

            await _unitOfWork.MenuPermissions.Update(menuPermission, cancellationToken);
            await _unitOfWork.SaveChanges();

            return Unit.Value;
        }
    }
}
