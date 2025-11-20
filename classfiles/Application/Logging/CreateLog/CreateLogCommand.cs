using MyWarehouse.Application.Common.Dependencies.DataAccess;
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

        public CreatelogCommandHandler(IUnitOfWork unitOfWork)
            => _unitOfWork = unitOfWork;

        public async Task<int> Handle(CreateLogCommand request, CancellationToken cancellationToken)
        {
            var logEntry = new AccessLog(
                id: request.Id,
                log: request.AccessLog.Trim(),
                user: request.User,
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
