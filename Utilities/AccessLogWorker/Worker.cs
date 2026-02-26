using System.Text;
using System.Text.Json;
using AccessLogWorker.Models;
using MongoDB.Driver;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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
        var rabbitHost = _configuration["RabbitMQ:Host"] ?? "localhost";
        var queueName = _configuration["RabbitMQ:Queue"] ?? "access-logs";
        var mongoConnectionString = _configuration["MongoDb:ConnectionString"] ?? "mongodb://localhost:27017";
        var mongoDatabaseName = _configuration["MongoDb:DatabaseName"] ?? "ListingDB";

        var mongoClient = new MongoClient(mongoConnectionString);
        var database = mongoClient.GetDatabase(mongoDatabaseName);
        var collection = database.GetCollection<AccessLog>("AccessLogs");

        var factory = new ConnectionFactory { HostName = rabbitHost };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var connection = await factory.CreateConnectionAsync(stoppingToken);
                await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

                await channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: stoppingToken);

                _logger.LogInformation("Listening for access log messages on queue '{Queue}'", queueName);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (_, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var log = JsonSerializer.Deserialize<AccessLog>(message);
                        if (log != null)
                        {
                            await collection.InsertOneAsync(log, cancellationToken: CancellationToken.None);
                            _logger.LogInformation("Saved access log: {Method} {Path} {StatusCode}", log.Method, log.Path, log.StatusCode);
                        }
                        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing access log message");
                        await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                    }
                };

                await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ connection error. Retrying in 5 seconds...");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
