using Azure.Messaging.ServiceBus;
using Messaging.Shared;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace MyWarehouse.WebApi.Messaging_Queue
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
                var json    = JsonSerializer.Serialize(message);
                var sbMsg   = new ServiceBusMessage(json)
                {
                    ContentType = "application/json"
                };

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
