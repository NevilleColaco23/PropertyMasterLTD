using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyWarehouse.Application.Common.Users;
using MyWarehouse.Application.Common.Users.DTO;
using MyWarehouse.Application.Common.Users.GetUsersByProperty;
using MyWarehouse.Application.UserActivity.Attributes;

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
        [LogList("Users")]
        public async Task<ActionResult<IEnumerable<GetUserDTO>>> GetAllUsers()
        {
            var query = new GetUsersQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get active users who have access to the given property, for @mention autocomplete.
        /// </summary>
        [HttpGet("by-property/{propertyId}")]
        [LogList("Users")]
        public async Task<ActionResult<List<GetUserDTO>>> GetByProperty(int propertyId)
        {
            var result = await _mediator.Send(new GetUsersByPropertyQuery { PropertyId = propertyId });
            return Ok(result);
        }
    }
}
