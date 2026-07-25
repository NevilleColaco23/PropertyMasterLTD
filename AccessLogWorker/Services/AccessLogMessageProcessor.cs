using Messaging.Shared.Models;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Domain.UserActivity;

namespace AccessLogWorker.Services
{
    public class AccessLogMessageProcessor : IAccessLogMessageProcessor
    {
        private readonly ILogger<AccessLogMessageProcessor> _logger;
        private readonly IUserActivityRepository _userActivityRepository;

        public AccessLogMessageProcessor(
            ILogger<AccessLogMessageProcessor> logger,
            IUserActivityRepository userActivityRepository)
        {
            _logger = logger;
            _userActivityRepository = userActivityRepository;
        }

        public async Task ProcessAsync(UserActivityEvent activityEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Processing activity event: UserId={UserId}, Action={Action}",
                activityEvent.UserId, activityEvent.Action);

            var log = new UserActivityLog
            {
                UserId       = activityEvent.UserId,
                Username     = activityEvent.Username,
                ActivityType = (ActivityType)activityEvent.ActivityType,
                EntityType   = activityEvent.EntityType,
                EntityId     = activityEvent.EntityId,
                Action       = activityEvent.Action,
                DisplayMessage = activityEvent.DisplayMessage,
                IPAddress    = activityEvent.IPAddress,
                Timestamp    = activityEvent.Timestamp,
                Metadata     = activityEvent.Metadata?
                                   .ToDictionary(kv => kv.Key, kv => (object)kv.Value)
            };

            var id = await _userActivityRepository.LogActivityAsync(log);

            _logger.LogInformation(
                "UserActivityLog saved to MongoDB with Id={Id}", id);
        }
    }
}
