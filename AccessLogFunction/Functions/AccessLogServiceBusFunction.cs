using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Messaging.Shared.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Domain.UserActivity;

namespace AccessLogFunction.Functions
{
    public class AccessLogServiceBusFunction
    {
        private readonly ILogger<AccessLogServiceBusFunction> _logger;
        private readonly IServiceProvider _serviceProvider;

        public AccessLogServiceBusFunction(
            ILogger<AccessLogServiceBusFunction> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Triggered every time a message arrives on the accesslog-queue.
        /// Deserializes the UserActivityEvent and saves it to MongoDB.
        /// Replaces the AccessLogConsumerWorker background service.
        /// </summary>
        [Function("AccessLogServiceBusFunction")]
        public async Task Run(
            [ServiceBusTrigger("%AccessLogQueueName%", Connection = "ServiceBusConnection")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("=== SERVICE BUS MESSAGE RECEIVED ===");
            _logger.LogInformation("MessageId: {Id}", message.MessageId);

            try
            {
                var body = message.Body.ToString();
                _logger.LogInformation("Body: {Body}", body);

                var activityEvent = JsonSerializer.Deserialize<UserActivityEvent>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (activityEvent is null)
                {
                    _logger.LogWarning("Message body could not be deserialized. Completing message to avoid poison-message loop.");
                    await messageActions.CompleteMessageAsync(message, cancellationToken);
                    return;
                }

                // Create a scope so the scoped IUserActivityRepository is properly disposed after each invocation
                await using var scope = _serviceProvider.CreateAsyncScope();
                var repository = scope.ServiceProvider.GetRequiredService<IUserActivityRepository>();

                var log = new UserActivityLog
                {
                    UserId         = activityEvent.UserId,
                    Username       = activityEvent.Username,
                    ActivityType   = (ActivityType)activityEvent.ActivityType,
                    EntityType     = activityEvent.EntityType,
                    EntityId       = activityEvent.EntityId,
                    Action         = activityEvent.Action,
                    DisplayMessage = activityEvent.DisplayMessage,
                    IPAddress      = activityEvent.IPAddress,
                    Timestamp      = activityEvent.Timestamp,
                    Metadata       = activityEvent.Metadata?
                                         .ToDictionary(kv => kv.Key, kv => (object)kv.Value)
                };

                var id = await repository.LogActivityAsync(log);

                _logger.LogInformation(
                    "UserActivityLog saved to MongoDB. Id={Id}, UserId={UserId}, Action={Action}",
                    id, activityEvent.UserId, activityEvent.Action);

                await messageActions.CompleteMessageAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Service Bus message {MessageId}. Abandoning for retry.", message.MessageId);
                await messageActions.AbandonMessageAsync(message, cancellationToken: cancellationToken);
            }
        }
    }
}
