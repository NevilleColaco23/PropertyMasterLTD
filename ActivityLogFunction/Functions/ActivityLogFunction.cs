using System.Text.Json;
using Messaging.Shared.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Domain.UserActivity;

namespace ActivityLogFunction.Functions;

/// <summary>
/// Azure Function that replaces AccessLogWorker.
/// Triggered by each message on the Service Bus queue and persists it to MongoDB.
/// </summary>
public class ActivityLogFunction
{
    private readonly ILogger<ActivityLogFunction> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ActivityLogFunction(ILogger<ActivityLogFunction> logger, IServiceProvider serviceProvider)
    {
        _logger          = logger;
        _serviceProvider = serviceProvider;
    }

    [Function(nameof(ActivityLogFunction))]
    public async Task RunAsync(
        [ServiceBusTrigger("%ServiceBusQueueName%", Connection = "ServiceBusConnection")]
        string messageBody,
        FunctionContext context)
    {
        _logger.LogInformation("=== SERVICE BUS MESSAGE RECEIVED ===");
        _logger.LogInformation("Body: {Body}", messageBody);

        var activityEvent = JsonSerializer.Deserialize<UserActivityEvent>(messageBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (activityEvent is null)
        {
            _logger.LogWarning("Message body could not be deserialized. Skipping.");
            return;
        }

        _logger.LogInformation(
            "Processing activity: UserId={UserId}, Action={Action}",
            activityEvent.UserId, activityEvent.Action);

        // Create a scope so scoped services (IUserActivityRepository) are resolved correctly
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

        _logger.LogInformation("UserActivityLog saved to MongoDB with Id={Id}", id);
    }
}
