using Azure.Messaging.ServiceBus;
using Messaging.Shared.Models;
using MongoDB.Driver;
using System.Text.Json;

namespace EmailWorker
{
    /// <summary>
    /// Listens on the Azure Service Bus email-queue and sends emails via Resend.
    /// MongoDB outbox is the persistent record; Service Bus is the trigger.
    /// </summary>
    public class Worker : BackgroundService
    {
        private readonly IMongoCollection<EmailOutboxMessage> _collection;
        private readonly IEmailSender _sender;
        private readonly ILogger<Worker> _logger;
        private readonly ServiceBusProcessor _processor;

        private const int MaxAttempts = 5;

        public Worker(
            IMongoDatabase db,
            IEmailSender sender,
            ILogger<Worker> logger,
            ServiceBusProcessor processor)
        {
            _collection = db.GetCollection<EmailOutboxMessage>(EmailOutboxCollection.Name);
            _sender     = sender;
            _logger     = logger;
            _processor  = processor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EmailWorker starting — listening on Service Bus queue.");

            _processor.ProcessMessageAsync += OnMessageAsync;
            _processor.ProcessErrorAsync   += OnErrorAsync;

            await _processor.StartProcessingAsync(stoppingToken);

            // Keep the worker alive until shutdown is requested
            try { await Task.Delay(Timeout.Infinite, stoppingToken); }
            catch (OperationCanceledException) { /* normal shutdown */ }

            await _processor.StopProcessingAsync();
            _logger.LogInformation("EmailWorker stopped.");
        }

        private async Task OnMessageAsync(ProcessMessageEventArgs args)
        {
            EmailQueuedEvent? emailEvent = null;
            try
            {
                emailEvent = JsonSerializer.Deserialize<EmailQueuedEvent>(args.Message.Body.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialise Service Bus message. Dead-lettering.");
                await args.DeadLetterMessageAsync(args.Message, "DeserializationFailed", ex.Message);
                return;
            }

            if (emailEvent == null)
            {
                _logger.LogWarning("Null email event received. Dead-lettering.");
                await args.DeadLetterMessageAsync(args.Message, "NullEvent", "Deserialized to null");
                return;
            }

            _logger.LogInformation("Received email event EmailId={Id} To={To}", emailEvent.EmailId, emailEvent.To);

            // Look up the outbox record — single source of truth
            var msg = await _collection.Find(x => x.Id == emailEvent.EmailId).FirstOrDefaultAsync();

            if (msg == null)
            {
                _logger.LogWarning("EmailOutbox record {Id} not found. Dead-lettering.", emailEvent.EmailId);
                await args.DeadLetterMessageAsync(args.Message, "RecordNotFound",
                    $"EmailOutbox record {emailEvent.EmailId} not found");
                return;
            }

            if (msg.Status == EmailOutboxStatus.Sent)
            {
                _logger.LogInformation("Email {Id} already sent — completing message.", msg.Id);
                await args.CompleteMessageAsync(args.Message);
                return;
            }

            await ProcessOneAsync(msg, args, args.CancellationToken);
        }

        private async Task ProcessOneAsync(
            EmailOutboxMessage msg,
            ProcessMessageEventArgs args,
            CancellationToken ct)
        {
            // Mark as processing
            await _collection.UpdateOneAsync(
                x => x.Id == msg.Id,
                Builders<EmailOutboxMessage>.Update
                    .Set(x => x.Status, EmailOutboxStatus.Processing)
                    .Inc(x => x.Attempts, 1),
                cancellationToken: ct);

            try
            {
                await _sender.SendHtmlAsync(msg.To, msg.Subject, msg.BodyHtml, ct);

                await _collection.UpdateOneAsync(
                    x => x.Id == msg.Id,
                    Builders<EmailOutboxMessage>.Update
                        .Set(x => x.Status, EmailOutboxStatus.Sent)
                        .Set(x => x.SentAtUtc, DateTime.UtcNow),
                    cancellationToken: ct);

                _logger.LogInformation("Email {Id} sent successfully to {To}", msg.Id, msg.To);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email {Id} to {To} (attempt {Attempts})",
                    msg.Id, msg.To, msg.Attempts + 1);

                if (msg.Attempts + 1 >= MaxAttempts)
                {
                    await _collection.UpdateOneAsync(
                        x => x.Id == msg.Id,
                        Builders<EmailOutboxMessage>.Update
                            .Set(x => x.Status, EmailOutboxStatus.Failed)
                            .Set(x => x.LastError, ex.Message),
                        cancellationToken: ct);

                    _logger.LogError("Email {Id} exceeded max attempts — dead-lettering.", msg.Id);
                    await args.DeadLetterMessageAsync(args.Message, "MaxAttemptsExceeded", ex.Message);
                }
                else
                {
                    // Abandon so Service Bus retries with its built-in backoff
                    await args.AbandonMessageAsync(args.Message);
                }
            }
        }

        private Task OnErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception,
                "Service Bus processor error. Source={Source} EntityPath={EntityPath}",
                args.ErrorSource, args.EntityPath);
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
            await base.StopAsync(cancellationToken);
        }
    }
}