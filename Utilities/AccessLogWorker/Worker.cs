using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace AccessLogWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rabbitMqHost = _configuration["RabbitMQ:Host"] ?? "rabbitmq";
        var factory = new ConnectionFactory { HostName = rabbitMqHost };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                var disconnected = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                connection.ConnectionShutdown += (_, _) => disconnected.TrySetResult(true);

                channel.QueueDeclare(
                    queue: "access-logs",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        _logger.LogInformation("Access log received: {Message}", message);
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing access log message, rejecting message.");
                        channel.BasicNack(ea.DeliveryTag, false, requeue: false);
                    }
                };

                channel.BasicConsume(queue: "access-logs", autoAck: false, consumer: consumer);

                _logger.LogInformation("AccessLogWorker connected to RabbitMQ on {Host}", rabbitMqHost);

                await Task.WhenAny(disconnected.Task, Task.Delay(Timeout.Infinite, stoppingToken));
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ connection error, retrying in 5 seconds...");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
