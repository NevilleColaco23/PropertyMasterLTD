using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Partners.CreatePartner;
using MyWarehouse.Application.Property.CreateProperty;
using MyWarehouse.Application.Property.GetProperty;

namespace MyWarehouse.Infrastructure.API.V1;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{v:apiVersion}/property")]
public class PropertyController : ControllerBase
{
    private readonly IMediator _mediator;

    public PropertyController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous]  // TEMPORARY: Remove this after testing!
    public async Task<ActionResult<IListResponseModel<GetPropertyDto>>> GetList([FromQuery] GetPropertyListQuery query)
    {
        try
        {
            Console.WriteLine($"=== PropertyController.GetList called ===");
            Console.WriteLine($"PageIndex: {query.PageIndex}, PageSize: {query.PageSize}, OrderBy: {query.OrderBy}");

            var result = await _mediator.Send(query);

            Console.WriteLine($"Results count: {result.Results?.Count() ?? 0}");
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== ERROR in PropertyController ===");
            Console.WriteLine($"Exception: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
            return BadRequest(new { error = ex.Message, innerError = ex.InnerException?.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreatePropertyCommand command)
        => Ok(await _mediator.Send(command));
}
