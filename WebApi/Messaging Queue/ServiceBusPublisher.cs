using System.Collections.Concurrent;
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
        // One sender (and AMQP link) per queue, reused across messages.
        private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();

        public ServiceBusPublisher(IOptions<ServiceBusOptions> options, ILogger<ServiceBusPublisher> logger)
        {
            // Short, bounded retries so an unreachable/deleted namespace fails fast
            // instead of holding a request (or a background send) for tens of seconds.
            var clientOptions = new ServiceBusClientOptions
            {
                RetryOptions = new ServiceBusRetryOptions
                {
                    MaxRetries = 1,
                    TryTimeout = TimeSpan.FromSeconds(5),
                    Delay = TimeSpan.FromMilliseconds(500),
                    MaxDelay = TimeSpan.FromSeconds(2)
                }
            };

            // The connection string may be empty/placeholder when Service Bus isn't configured
            // (local dev); in that case nothing is ever sent, so don't build a client.
            var connectionString = options.Value.ConnectionString;
            if (!string.IsNullOrWhiteSpace(connectionString) && !connectionString.StartsWith("<"))
            {
                _client = new ServiceBusClient(connectionString, clientOptions);
            }
            _logger = logger;
        }

        public async Task SendAsync<T>(T message, string queueName, CancellationToken cancellationToken = default)
        {
            if (_client == null)
            {
                return;
            }

            try
            {
                var sender = _senders.GetOrAdd(queueName, name => _client.CreateSender(name));
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
            foreach (var sender in _senders.Values)
            {
                await sender.DisposeAsync();
            }

            if (_client != null)
            {
                await _client.DisposeAsync();
            }
        }
    }
}
