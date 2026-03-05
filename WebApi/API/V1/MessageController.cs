using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.Menus;
using MyWarehouse.Application.Common.Menus.DTO;
using MyWarehouse.Application.Common.Messages;

namespace MyWarehouse.WebApi.API.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiversion}/Messages")]
    public class MessagesController : Controller
    {
        private readonly IMediator _mediator;

        public MessagesController(IMediator mediator) => _mediator = mediator;

        [HttpGet("GetSnackbarmessages")]
        public async Task<ActionResult<IListResponseModel<GetMenuListDTO>>> GetSystemMessages([FromQuery] GetSystemMessagesForSnackbarQuery query)
            => Ok(await _mediator.Send(query));
    }
}
