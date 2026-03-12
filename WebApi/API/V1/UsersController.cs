using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyWarehouse.Application.Common.Users;
using MyWarehouse.Application.Common.Users.DTO;

namespace MyWarehouse.WebApi.API.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        [HttpGet("getall")]
        public async Task<ActionResult<IEnumerable<GetUserDTO>>> GetAllUsers()
        {
            var query = new GetUsersQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
