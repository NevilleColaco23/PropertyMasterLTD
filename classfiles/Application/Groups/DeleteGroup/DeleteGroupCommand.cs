using MyWarehouse.Application.Common.Dependencies.DataAccess;

namespace MyWarehouse.Application.Groups.DeleteGroup;

public class DeleteGroupCommand : IRequest<bool>
{
    public int Id { get; init; }
}

public class DeleteGroupCommandHandler : IRequestHandler<DeleteGroupCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteGroupCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _unitOfWork.Groups!.GetByIdAsync(request.Id);

        if (group is null)
        {
            return false;
        }

        _unitOfWork.Groups!.Remove(group);

        return true;
    }
}
