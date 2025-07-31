using MyWarehouse.Application.Bookings.GetBookings;
using MyWarehouse.Application.Common.Bookings;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.Menus;
using GetBookingsListQuery = MyWarehouse.Application.Common.Bookings.GetBookingsListQuery;

namespace MyWarehouse.WebApi.API.DomainControllers
{
    //[Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{v:apiVersion}/menu")]
    public class BookingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BookingsController(IMediator mediator) => _mediator = mediator;

        [HttpGet("GetBookings")]
        public async Task<ActionResult<IListResponseModel<GetBookingsListDTO>>> GetBookings([FromQuery] GetBookingsListQuery query)
            => Ok(await _mediator.Send(query));
    }
}
