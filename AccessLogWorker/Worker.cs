using System.Text.Json;
using Azure.Messaging.ServiceBus;
using AccessLogWorker.Services;
using Messaging.Shared;
using Messaging.Shared.Models;
using Microsoft.Extensions.Options;

namespace AccessLogWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ServiceBusOptions _options;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private ServiceBusClient? _client;
        private ServiceBusProcessor? _processor;

        public Worker(
            ILogger<Worker> logger,
            IOptions<ServiceBusOptions> options,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _options = options.Value;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting AccessLogWorker - connecting to Azure Service Bus queue: {Queue}",
                _options.AccessLogQueueName);

            _client = new ServiceBusClient(_options.ConnectionString);

            _processor = _client.CreateProcessor(_options.AccessLogQueueName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 1
            });

            _processor.ProcessMessageAsync += OnMessageReceivedAsync;
            _processor.ProcessErrorAsync  += OnErrorAsync;

            await _processor.StartProcessingAsync(cancellationToken);

            _logger.LogInformation("AccessLogWorker started. Listening on queue: {Queue}", _options.AccessLogQueueName);

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Keep the worker alive — the processor runs on its own background threads
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task OnMessageReceivedAsync(ProcessMessageEventArgs args)
        {
            try
            {
                var body = args.Message.Body.ToString();

                _logger.LogInformation("=== SERVICE BUS MESSAGE RECEIVED ===");
                _logger.LogInformation("MessageId: {Id}", args.Message.MessageId);
                _logger.LogInformation("Body: {Body}", body);

                var activityEvent = JsonSerializer.Deserialize<UserActivityEvent>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (activityEvent is null)
                {
                    _logger.LogWarning("Message body could not be deserialized. Completing (dead-letter via retry policy).");
                    await args.CompleteMessageAsync(args.Message);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IAccessLogMessageProcessor>();
                await processor.ProcessAsync(activityEvent, args.CancellationToken);

                _logger.LogInformation("Message processed successfully. Completing...");
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Service Bus message. Abandoning for retry.");
                await args.AbandonMessageAsync(args.Message);
            }
        }

        private Task OnErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception,
                "Azure Service Bus error. Source: {Source}, EntityPath: {EntityPath}",
                args.ErrorSource, args.EntityPath);
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_processor is not null)
            {
                await _processor.StopProcessingAsync(cancellationToken);
                await _processor.DisposeAsync();
            }
            if (_client is not null)
            {
                await _client.DisposeAsync();
            }
            await base.StopAsync(cancellationToken);
        }
    }
}