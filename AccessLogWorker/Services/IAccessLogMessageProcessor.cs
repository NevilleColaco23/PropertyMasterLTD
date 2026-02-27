using Messaging.Shared.Models;

namespace AccessLogWorker.Services
{
    public interface IAccessLogMessageProcessor
    {
        Task ProcessMessageAsync(AccessLogEvent logEvent, CancellationToken cancellationToken);
    }
}
