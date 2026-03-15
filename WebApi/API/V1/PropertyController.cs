using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Partners.CreatePartner;
using MyWarehouse.Application.Property.CreateProperty;
using MyWarehouse.Application.Property.GetProperty;
using MyWarehouse.Application.Property.UpdateProperty;
using MyWarehouse.Application.Property.DeleteProperty;
using MyWarehouse.Application.UserActivity.Attributes;

namespace MyWarehouse.Infrastructure.API.V1;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{v:apiVersion}/property")]
public class PropertyController : ControllerBase
{
    private readonly IMediator _mediator;

    public PropertyController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get ALL properties - Used by Property Master admin page
    /// Returns every property in the database for management purposes
    /// </summary>
    [HttpGet]
    [AllowAnonymous]  // TEMPORARY: Remove this after testing!
    [LogList("Properties")]
    public async Task<ActionResult<IListResponseModel<GetPropertyDto>>> GetList([FromQuery] GetPropertyListQuery query)
    {
        try
        {
            Console.WriteLine($"=== PropertyController.GetList (ALL PROPERTIES) ===");
            var result = await _mediator.Send(query);
            Console.WriteLine($"Properties returned: {result.Results?.Count() ?? 0}");
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== ERROR in PropertyController.GetList ===");
            Console.WriteLine($"Exception: {ex.Message}");
            return BadRequest(new { error = ex.Message, innerError = ex.InnerException?.Message });
        }
    }

    /// <summary>
    /// Get ONLY properties user has access to - Used by Property Selector
    /// Filters based on User.PropertyAccessList
    /// </summary>
    [HttpGet("accessible")]
    [AllowAnonymous]  // TEMPORARY: Remove this after testing!
    [LogList("Properties", Description = "User accessed their property list")]
    public async Task<ActionResult<IListResponseModel<GetPropertyDto>>> GetAccessibleProperties([FromQuery] GetUserAccessiblePropertiesQuery query)
    {
        try
        {
            Console.WriteLine($"=== PropertyController.GetAccessibleProperties (USER ACCESS FILTERED) ===");
            var result = await _mediator.Send(query);
            Console.WriteLine($"Properties returned: {result.Results?.Count() ?? 0}");
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== ERROR in PropertyController.GetAccessibleProperties ===");
            Console.WriteLine($"Exception: {ex.Message}");
            return BadRequest(new { error = ex.Message, innerError = ex.InnerException?.Message });
        }
    }

    [HttpGet("{id}")]
    [LogView("Property")]
    public async Task<ActionResult<GetPropertyDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetPropertyListQuery());
        var property = result.Results?.FirstOrDefault(p => p.Id == id);

        if (property == null)
            return NotFound();

        return Ok(property);
    }

    [HttpPost]
    [LogCreate("Property")]
    public async Task<ActionResult<int>> Create(CreatePropertyCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id}")]
    [LogUpdate("Property")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdatePropertyCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [LogDelete("Property")]
    public async Task<ActionResult> Delete(int id)
    {
        await _mediator.Send(new DeletePropertyCommand { Id = id });
        return NoContent();
    }
}
