using MyWarehouse.Application.NewFolder.CreateLog;

namespace MyWarehouse.Infrastructure.API.V1
{

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{v:apiVersion}/accessLog")]
    public class AccessLoggingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccessLoggingController(IMediator mediator) => _mediator = mediator;

        [HttpPost("accessLog")]
        public async Task<ActionResult<int>> Create(CreateLogCommand command)
            => Ok(await _mediator.Send(command));
    }
}