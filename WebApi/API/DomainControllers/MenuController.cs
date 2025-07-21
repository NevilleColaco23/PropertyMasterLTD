using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.Menus;
using MyWarehouse.Application.Common.Searchbox;

namespace MyWarehouse.WebApi.API.DomainControllers
{
    //[Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{v:apiVersion}/menu")]
    public class MenuController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MenuController(IMediator mediator) => _mediator = mediator;

        [HttpGet("GetListByUserId")]
        public async Task<ActionResult<IListResponseModel<GetMenuListDTO>>> GetListByUserId([FromQuery] GetMenuListQuery query)
            =>      Ok(await _mediator.Send(query));

        [HttpGet("search")]
        public async Task<ActionResult<IListResponseModel<GetSearchResultsDTO>>> GetSearchResults([FromQuery] GetSearchResultsQuery query)
            => Ok(await _mediator.Send(query));
    }
}
