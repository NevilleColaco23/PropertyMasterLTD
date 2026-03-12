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

        public DeleteMenuPermissionCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(DeleteMenuPermissionCommand request, CancellationToken cancellationToken)
        {
            var permission = await _unitOfWork.MenuPermissions.GetByIdAsync(request.Id);
            if (permission != null)
            {
                _unitOfWork.MenuPermissions.Remove(permission);
                await _unitOfWork.SaveChanges();
            }

            return Unit.Value;
        }
    }
}
