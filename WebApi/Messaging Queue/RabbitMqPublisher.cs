using Amazon.Runtime.Internal;
using Azure.Core;
using Messaging.Shared;
using Microsoft.Extensions.Options;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Domain.AccessLog;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace MyWarehouse.WebApi.Messaging_Queue
{
    public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
    {
        private readonly Messaging.Shared.RabbitMqOptions _options;
        private readonly ILogger<RabbitMqPublisher> _logger;
        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMqPublisher(IOptions<Messaging.Shared.RabbitMqOptions> options, ILogger<RabbitMqPublisher> logger)
        {
            _options = options.Value;
            _logger = logger;

            try
            {
                _logger.LogInformation("Initializing RabbitMQ connection to {Host}:{Port}", _options.Host, _options.Port);

                var factory = new ConnectionFactory
                {
                    HostName = _options.Host,
                    Port = _options.Port,
                    UserName = _options.Username,
                    Password = _options.Password,
                    VirtualHost = _options.VirtualHost
                };

                _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
                _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
                _channel.ExchangeDeclareAsync(_options.Exchange, ExchangeType.Direct, durable: true).GetAwaiter().GetResult();

                #region 1.1 moreinfo
                _channel.QueueDeclareAsync(
                    queue: _options.Queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null).GetAwaiter().GetResult();

                // Bind queue to exchange with routing key
                _channel.QueueBindAsync(
                    queue: _options.Queue,
                    exchange: _options.Exchange,
                    routingKey: _options.RoutingKey).GetAwaiter().GetResult();
                #endregion

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

        public void PublishAccessLogEvent(AccessLog accessLog)
        {
            if (_channel == null) throw new InvalidOperationException("RabbitMQ channel is not initialized.");

            try
            {
                var evt = new Messaging.Shared.Models.AccessLogEvent
                {
                    TimestampUtc = accessLog.TimeStamp,
                    Method = accessLog.Details,
                    Path = accessLog.Log,
                    StatusCode = 200,
                    DurationMs = 0,
                    UserId = accessLog.UserID.ToString(),
                    Username = null,
                    TraceId = null,
                    ClientIp = null,
                    UserAgent = null,
                    Action = accessLog.Action
                };

                var json = JsonSerializer.Serialize(evt);
                var body = Encoding.UTF8.GetBytes(json);

                _logger.LogInformation("Publishing AccessLog event to {Exchange}/{RoutingKey}: LogId={LogId}, User={UserId}, Action={Action}",
                    _options.Exchange, _options.RoutingKey, accessLog.Id, accessLog.UserID, accessLog.Action);

                _channel.BasicPublishAsync(_options.Exchange, _options.RoutingKey, body).GetAwaiter().GetResult();

                _logger.LogInformation("AccessLog event published successfully: LogId={LogId}", accessLog.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish AccessLog event: LogId={LogId}", accessLog.Id);
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