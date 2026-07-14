using global::Messaging.Shared;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace testAngularAPI.Server.Infrastructure.Messaging
{
    public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
    {
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqPublisher> _logger;
        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMqPublisher(IOptions<RabbitMqOptions> options, ILogger<RabbitMqPublisher> logger)
        {
            _options = options.Value;
            _logger = logger;

            try
            {
                _logger.LogInformation("Initializing RabbitMQ connection to {Host}:{Port} (SSL: {UseSsl})", _options.Host, _options.Port, _options.UseSsl);

                var factory = new ConnectionFactory
                {
                    HostName = _options.Host,
                    Port = _options.Port,
                    UserName = _options.Username,
                    Password = _options.Password,
                    VirtualHost = _options.VirtualHost
                };

                if (_options.UseSsl)
                {
                    factory.Ssl = new RabbitMQ.Client.SslOption
                    {
                        Enabled = true,
                        ServerName = _options.Host,
                        AcceptablePolicyErrors = System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch |
                                                  System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors
                    };
                }

                _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
                _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
                _channel.ExchangeDeclareAsync(_options.Exchange, ExchangeType.Direct, durable: true).GetAwaiter().GetResult();

                _channel.QueueDeclareAsync(
                    queue: _options.Queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null).GetAwaiter().GetResult();

                _channel.QueueBindAsync(
                    queue: _options.Queue,
                    exchange: _options.Exchange,
                    routingKey: _options.RoutingKey).GetAwaiter().GetResult();

                _logger.LogInformation("RabbitMQ connection initialized successfully. Exchange: {Exchange}", _options.Exchange);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
                throw;
            }
        }

        public void Publish<T>(T message, string exchange, string routingKey)
        {
            if (_channel == null) throw new InvalidOperationException("RabbitMQ channel is not initialized.");

            try
            {
                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                _logger.LogInformation("Publishing message to exchange: {Exchange}, routingKey: {RoutingKey}, size: {Size} bytes",
                    exchange, routingKey, body.Length);

                _channel.BasicPublishAsync(exchange, routingKey, body).GetAwaiter().GetResult();

                _logger.LogInformation("Message published successfully to {Exchange}/{RoutingKey}", exchange, routingKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish message to {Exchange}/{RoutingKey}", exchange, routingKey);
                throw;
            }
        }

        public void Dispose()
        {
            if (_channel is not null)
            {
                try { _channel.CloseAsync().GetAwaiter().GetResult(); } catch { }
            }
            if (_connection is not null)
            {
                try { _connection.CloseAsync().GetAwaiter().GetResult(); } catch { }
            }
        }
    }
}
