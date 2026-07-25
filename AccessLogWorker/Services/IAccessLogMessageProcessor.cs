using Messaging.Shared.Models;

namespace AccessLogWorker.Services
{
    public interface IAccessLogMessageProcessor
    {
        Task ProcessAsync(UserActivityEvent activityEvent, CancellationToken cancellationToken);
    }
}
