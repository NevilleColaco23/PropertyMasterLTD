using MyWarehouse.Application.SystemSettings.GetFeedSettings;
using MyWarehouse.Application.SystemSettings.SaveFeedSettings;

namespace MyWarehouse.Infrastructure.API.V1;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{v:apiVersion}/feed-settings")]
public class FeedSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FeedSettingsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<FeedSettingsDto>> Get()
        => Ok(await _mediator.Send(new GetFeedSettingsQuery()));

    [HttpPut]
    public async Task<ActionResult> Save(SaveFeedSettingsCommand command)
    {
        await _mediator.Send(command);

        return NoContent();
    }
}
