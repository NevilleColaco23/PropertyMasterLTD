using Microsoft.AspNetCore.Mvc;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.MenuPermissions;
using MyWarehouse.Application.UserActivity.Attributes;

namespace MyWarehouse.WebApi.API.V1
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}/menupermissions")]
    public class MenuPermissionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MenuPermissionsController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [LogList("Menu Permissions")]
        public async Task<ActionResult<IListResponseModel<GetMenuPermissionDTO>>> GetMenuPermissions([FromQuery] GetMenuPermissionsQuery query)
            => Ok(await _mediator.Send(query));

        [HttpGet("{id}")]
        [LogView("Menu Permission")]
        public async Task<ActionResult<GetMenuPermissionDTO>> GetMenuPermissionById(int id)
        {
            var result = await _mediator.Send(new GetMenuPermissionByIdQuery { Id = id });
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [LogCreate("Menu Permission")]
        public async Task<ActionResult<int>> CreateMenuPermission([FromBody] CreateMenuPermissionCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetMenuPermissionById), new { id }, id);
        }

        [HttpPut("{id}")]
        [LogUpdate("Menu Permission")]
        public async Task<ActionResult> UpdateMenuPermission(int id, [FromBody] UpdateMenuPermissionCommand command)
        {
            if (id != command.Id)
                return BadRequest("ID mismatch");

            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [LogDelete("Menu Permission")]
        public async Task<ActionResult> DeleteMenuPermission(int id)
        {
            await _mediator.Send(new DeleteMenuPermissionCommand { Id = id });
            return NoContent();
        }
    }
}
