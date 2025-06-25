using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.Menus;

namespace MyWarehouse.WebApi.API.DomainControllers
{
    //[Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{v:apiVersion}/Menu")]
    public class MenuController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MenuController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<ActionResult<IListResponseModel<GetMenuListDTO>>> GetList([FromQuery] GetMenuListQuery query)
            => Ok(await _mediator.Send(query));

    }
}
