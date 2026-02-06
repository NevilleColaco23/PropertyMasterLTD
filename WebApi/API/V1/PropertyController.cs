using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Partners.CreatePartner;
using MyWarehouse.Application.Property.CreateProperty;
using MyWarehouse.Application.Property.GetProperty;

namespace MyWarehouse.Infrastructure.API.V1;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("v{v:apiVersion}/property")]
public class PropertyController : ControllerBase
{
    private readonly IMediator _mediator;

    public PropertyController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<IListResponseModel<GetPropertyDto>>> GetList([FromQuery] GetPropertyListQuery query)
        => Ok(await _mediator.Send(query));

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreatePropertyCommand command)
        => Ok(await _mediator.Send(command));
}
