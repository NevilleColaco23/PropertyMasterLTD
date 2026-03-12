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

        public UpdateMenuPermissionCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(UpdateMenuPermissionCommand request, CancellationToken cancellationToken)
        {
            var menuPermission = new MenusPermissions
            {
                Id = request.Id,
                UserId = request.UserId,
                MenuID = request.MenuId,
                AccessLevel = request.AccessLevel,
                From = request.From,
                To = request.To
            };

            await _unitOfWork.MenuPermissions.Update(menuPermission, cancellationToken);
            await _unitOfWork.SaveChanges();

            return Unit.Value;
        }
    }
}
