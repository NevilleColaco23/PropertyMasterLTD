using MyWarehouse.Application.Groups.CreateGroup;
using MyWarehouse.Application.Groups.DeleteGroup;
using MyWarehouse.Application.Groups.GetGroupsList;
using MyWarehouse.Application.Groups.UpdateGroup;
using MyWarehouse.Application.UserActivity.Attributes;

namespace MyWarehouse.Infrastructure.API.V1;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{v:apiVersion}/groups")]
public class GroupsController : ControllerBase
{
    private readonly IMediator _mediator;

    public GroupsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [LogList("Groups")]
    public async Task<ActionResult<List<GroupDto>>> GetList()
        => Ok(await _mediator.Send(new GetGroupsListQuery()));

    [HttpPost]
    [LogCreate("Group")]
    public async Task<ActionResult<int>> Create(CreateGroupCommand command)
        => Ok(await _mediator.Send(command));

    [HttpPut("{id}")]
    [LogUpdate("Group")]
    public async Task<ActionResult> Update(int id, UpdateGroupCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        var updated = await _mediator.Send(command);

        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    [LogDelete("Group")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _mediator.Send(new DeleteGroupCommand { Id = id });

        return deleted ? NoContent() : NotFound();
    }
}
