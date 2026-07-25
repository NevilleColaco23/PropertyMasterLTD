using Azure.Messaging.ServiceBus;
using global::Messaging.Shared;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace testAngularAPI.Server.Infrastructure.Messaging
{
    public class ServiceBusPublisher : IServiceBusPublisher, IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ILogger<ServiceBusPublisher> _logger;

        public ServiceBusPublisher(IOptions<ServiceBusOptions> options, ILogger<ServiceBusPublisher> logger)
        {
            _client = new ServiceBusClient(options.Value.ConnectionString);
            _logger = logger;
        }

        public async Task SendAsync<T>(T message, string queueName, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var sender = _client.CreateSender(queueName);
                var json  = JsonSerializer.Serialize(message);
                var sbMsg = new ServiceBusMessage(json) { ContentType = "application/json" };

                await sender.SendMessageAsync(sbMsg, cancellationToken);
                _logger.LogInformation("Message sent to Service Bus queue '{Queue}'", queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message to Service Bus queue '{Queue}'", queueName);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _client.DisposeAsync();
        }
    }
}
