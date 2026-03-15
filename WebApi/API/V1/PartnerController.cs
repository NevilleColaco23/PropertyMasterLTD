using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Partners.CreatePartner;
using MyWarehouse.Application.Partners.DeletePartner;
using MyWarehouse.Application.Partners.GetPartnerDetails;
using MyWarehouse.Application.Partners.GetPartnersList;
using MyWarehouse.Application.Partners.UpdatePartner;
using MyWarehouse.Application.UserActivity.Attributes;

namespace MyWarehouse.Infrastructure.API.V1;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{v:apiVersion}/partners")]
public class PartnerController : ControllerBase
{
    private readonly IMediator _mediator;

    public PartnerController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [LogCreate("Partner")]
    public async Task<ActionResult<int>> Create(CreatePartnerCommand command)
        => Ok(await _mediator.Send(command));

    [HttpGet]
    [LogList("Partners")]
    public async Task<ActionResult<IListResponseModel<PartnerDto>>> GetList([FromQuery] ListQueryModel<PartnerDto> query)
        => Ok(await _mediator.Send(query));

    [HttpGet("{id}")]
    [LogView("Partner")]
    public async Task<ActionResult<PartnerDetailsDto>> Get(int id)
        => Ok(await _mediator.Send(new GetPartnerDetailsQuery() { Id = id }));

    [HttpDelete("{id}")]
    [LogDelete("Partner")]
    public async Task<ActionResult> Delete(int id)
    {
        await _mediator.Send(new DeletePartnerCommand() { Id = id });

        return NoContent();
    }

    [HttpPut("{id}")]
    [LogUpdate("Partner")]
    public async Task<ActionResult> Update(int id, UpdatePartnerCommand command)
    {
        if (id != command.Id) return BadRequest();

        await _mediator.Send(command);

        return NoContent();
    }
}
