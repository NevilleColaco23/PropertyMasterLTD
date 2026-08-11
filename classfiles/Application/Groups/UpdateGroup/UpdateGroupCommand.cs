using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Domain.Groups;

namespace MyWarehouse.Application.Groups.UpdateGroup;

public class UpdateGroupCommand : IRequest<bool>
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string Description { get; init; } = string.Empty;
}

public class UpdateGroupCommandValidator : AbstractValidator<UpdateGroupCommand>
{
    public UpdateGroupCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class UpdateGroupCommandHandler : IRequestHandler<UpdateGroupCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateGroupCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _unitOfWork.Groups!.GetByIdAsync(request.Id);

        if (group is null)
        {
            return false;
        }

        group.Name = request.Name.Trim();
        group.Description = request.Description.Trim();

        await _unitOfWork.Groups!.Update(group, cancellationToken);

        return true;
    }
}
