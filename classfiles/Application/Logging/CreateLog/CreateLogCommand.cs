using Messaging.Shared;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Dependencies.Services;
using MyWarehouse.Domain.AccessLog;

namespace MyWarehouse.Application.NewFolder.CreateLog
{
    public class CreateLogCommand : IRequest<int>
    {
        public int Id { get; init; }
        public string AccessLog { get; init; } = null!;
        public int User { get; init; }
        public DateTime TimeStamp { get; init; }
        public string Action { get; init; } = null!;
        public string Detail { get; init; } = null!;
        public string? IpAddress { get; init; }
        public string? UserAgent { get; init; }
    }

    public class CreatelogCommandHandler : IRequestHandler<CreateLogCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IRabbitMqPublisher _rabbitMqPublisher;

        public CreatelogCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IRabbitMqPublisher rabbitMqPublisher,
            IRabbitMqPublisher publisher, Microsoft.Extensions.Options.IOptions<RabbitMqOptions> options)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _rabbitMqPublisher = rabbitMqPublisher;
        }

        public async Task<int> Handle(CreateLogCommand request, CancellationToken cancellationToken)
        {
            string userIdString = _currentUserService.UserId ?? "0";

            if (!int.TryParse(userIdString, out int parsedUserId))
            {
                // TODO: map "System" to -1, "Anonymous" to -2
                parsedUserId = 0;
            }

            var evt = new AccessLog(
                id: request.Id,
                log: request.AccessLog.Trim(),
                user: parsedUserId,
                time: request.TimeStamp,
                action: request.Action,
                details: request.Detail,
                source: "Direct",
                ipAddress: request.IpAddress,
                userAgent: request.UserAgent
            );

            // Publish to RabbitMQ (consumer will save to DB)
            try
            {
                Console.WriteLine("Client IP Detected at " + DateTime.Now + " : " + request.IpAddress);

                // Convert to AccessLogEvent for RabbitMQ
                var accessLogEvent = new Messaging.Shared.Models.AccessLogEvent
                {
                    TimestampUtc = evt.TimeStamp,
                    Method = evt.Details,
                    Path = evt.Log,
                    StatusCode = 200,
                    DurationMs = 0,
                    UserId = evt.UserID.ToString(),
                    Username = null,
                    TraceId = null,
                    ClientIp = evt.IpAddress,
                    UserAgent = evt.UserAgent,
                    Action = evt.Action,
                    Id = evt.Id
                };

                _rabbitMqPublisher.Publish(accessLogEvent, "accesslog.exchange", "accesslog");
                return evt.Id;
            }
            catch (Exception ex)
            {
                // Log error, don't throw
                // _logger?.LogError(ex, "Failed to push access log event to RabbitMQ");
                return 0;
            }
        }
    }
}
