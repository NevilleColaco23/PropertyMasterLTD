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
                ipAddress: request.IpAddress
            );

            // Publish to RabbitMQ (consumer will save to DB)
            try
            {
                _rabbitMqPublisher.PublishAccessLogEvent(evt);
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
