using MyWarehouse.Application.Common.Dependencies.DataAccess;

namespace MyWarehouse.Application.Groups.GetGroupsList;

public class GetGroupsListQuery : IRequest<List<GroupDto>>
{
}

public class GroupDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public class GetGroupsListQueryHandler : IRequestHandler<GetGroupsListQuery, List<GroupDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetGroupsListQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<GroupDto>> Handle(GetGroupsListQuery request, CancellationToken cancellationToken)
    {
        var groups = await _unitOfWork.Groups!.GetAllAsync();

        return groups.Select(g => new GroupDto
        {
            Id = g.Id,
            Name = g.Name,
            Description = g.Description
        }).ToList();
    }
}
