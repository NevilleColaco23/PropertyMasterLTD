using MyWarehouse.Application.NewFolder.CreateLog;

namespace MyWarehouse.Infrastructure.API.V1
{

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}/accessLog")]
    public class AccessLoggingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccessLoggingController(IMediator mediator) => _mediator = mediator;

        [HttpPost("accessLog")]
        public async Task<ActionResult<int>> Create(CreateLogCommand command)
        {
            // Get client IP address and User-Agent
            var ipAddress = GetClientIpAddress();
            var userAgent = GetUserAgent();

            // Create a new command with the IP address and User-Agent
            var commandWithClientInfo = new CreateLogCommand
            {
                Id = command.Id,
                AccessLog = command.AccessLog,
                User = command.User,
                TimeStamp = command.TimeStamp,
                Action = command.Action,
                Detail = command.Detail,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            return Ok(await _mediator.Send(commandWithClientInfo));
        }

        private string? GetClientIpAddress()
        {
            // Check for X-Forwarded-For header (common with proxies and load balancers)
            var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                // X-Forwarded-For can contain multiple IPs, take the first one
                return forwardedFor.Split(',')[0].Trim();
            }

            // Check for X-Real-IP header (used by some proxies like nginx)
            var realIp = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Fall back to RemoteIpAddress
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        private string? GetUserAgent()
        {
            return HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
        }
    }
}