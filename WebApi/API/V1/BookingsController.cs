
using Microsoft.AspNetCore.Mvc;
using MyWarehouse.Application.Common.Bookings;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Partners.CreatePartner;

namespace MyWarehouse.WebApi.API.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiversion}/Bookings")]
    public class BookingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BookingsController(IMediator mediator) => _mediator = mediator;

        [HttpGet("GetBookings")]
        public async Task<ActionResult<IListResponseModel<GetBookingsListDTO>>> GetBookings([FromQuery] GetBookingsListQuery query)
            => Ok(await _mediator.Send(query));
    }
}
