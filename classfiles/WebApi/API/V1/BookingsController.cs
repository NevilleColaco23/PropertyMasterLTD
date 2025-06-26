
using MyWarehouse.Application.Partners.CreatePartner;

namespace MyWarehouse.WebApi.API.V1
{
    //[ApiController]
    //[ApiVersion("1.0")]
    //[Route("v{v:apiVersion}/partners")]
    public class BookingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BookingsController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<ActionResult<int>> GetBooking(CreatePartnerCommand command)
            => Ok(await _mediator.Send(command));
    }
}
