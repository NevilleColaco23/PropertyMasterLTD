using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Dependencies.Services;
using MyWarehouse.Domain.Groups;

namespace MyWarehouse.Application.Groups.CreateGroup;

public class CreateGroupCommand : IRequest<int>
{
    public string Name { get; init; } = null!;
    public string Description { get; init; } = string.Empty;
}

public class CreateGroupCommandValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateGroupCommandHandler : IRequestHandler<CreateGroupCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateGroupCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<int> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = int.TryParse(_currentUserService.UserId, out var userId) ? userId : 0;

        var group = new Group(request.Name.Trim(), request.Description.Trim())
        {
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserId
        };

        var added = await _unitOfWork.Groups!.Add(group, cancellationToken);

        return added.Id;
    }
}
