using Messaging.Shared;
using Messaging.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace MyWarehouse.WebApi.API.Test
{
    [Route("api/v{v:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly Messaging.Shared.RabbitMqOptions _options;

        public TestController(IOptions<Messaging.Shared.RabbitMqOptions> options)
        {
            _options = options.Value;
        }

        [HttpGet]
        public IActionResult GetTest()
        {
            return Ok(new 
            { 
                message = "Test endpoint is working!", 
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            });
        }

        [HttpPost]
        public async Task<IActionResult> PublishTestAccessLog()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _options.Host,
                    Port = _options.Port,
                    UserName = _options.Username,
                    Password = _options.Password,
                    VirtualHost = _options.VirtualHost
                };

                // Configure SSL for CloudAMQP
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

                var connection = await factory.CreateConnectionAsync();
                var channel = await connection.CreateChannelAsync();

                try
                {
                    await channel.ExchangeDeclareAsync(_options.Exchange, ExchangeType.Direct, durable: true, autoDelete: false);

                    var testEvent = new AccessLogEvent
                    {
                        TimestampUtc = DateTime.UtcNow,
                        Method = "GET",
                        Path = "/test-accesslog",
                        StatusCode = 200,
                        DurationMs = 123,
                        Username = "testuser",
                        UserId = "testuserid",
                        TraceId = Guid.NewGuid().ToString(),
                        ClientIp = "127.0.0.1",
                        UserAgent = "TestAgent/1.0"
                    };

                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(testEvent));

                    await channel.BasicPublishAsync(
                        exchange: _options.Exchange,
                        routingKey: _options.RoutingKey,
                        body: body
                    );

                    return Ok(new { message = "Test message published to RabbitMQ." });
                }
                finally
                {
                    await channel.CloseAsync();
                    await connection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"RabbitMQ publish failed: {ex.Message}");
            }
        }
    }
}
