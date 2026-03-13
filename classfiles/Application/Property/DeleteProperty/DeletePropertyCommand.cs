using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Dependencies.Services;
using MyWarehouse.Application.Common.Audit;

namespace MyWarehouse.Application.Property.DeleteProperty;

public class DeletePropertyCommand : IRequest<Unit>
{
    public int Id { get; init; }
}

public class DeletePropertyCommandHandler : IRequestHandler<DeletePropertyCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public DeletePropertyCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Unit> Handle(DeletePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = await _unitOfWork.Properties.GetByIdAsync(request.Id);
        
        if (property != null)
        {
            var currentUserId = int.TryParse(_currentUserService.UserId, out var userId) ? userId : 0;

            _unitOfWork.Properties.Remove(property);
            await _unitOfWork.SaveChanges();

            // Log the delete operation to audit trail
            await _auditService.LogDelete(
                "Properties",
                request.Id,
                property,
                currentUserId
            );
        }

        return Unit.Value;
    }
}
