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
    }

    public class CreatelogCommandHandler : IRequestHandler<CreateLogCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreatelogCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<int> Handle(CreateLogCommand request, CancellationToken cancellationToken)
        {
            string userIdString = _currentUserService.UserId ?? "0";

            if (!int.TryParse(userIdString, out int parsedUserId))
            {
                // TODO: map "System" to -1, "Anonymous" to -2
                parsedUserId = 0;
            }

            var logEntry = new AccessLog(
                id: request.Id,
                log: request.AccessLog.Trim(),
                user: parsedUserId,
                time: request.TimeStamp,
                action:request.Action,
                details:request.Detail
                );

            _unitOfWork.AccessLogs?.Add(logEntry);
            await _unitOfWork.SaveChanges();

            return logEntry.Id;
        }
    }
}
