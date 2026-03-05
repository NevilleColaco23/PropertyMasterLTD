using Messaging.Shared.Models;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Domain.AccessLog;

namespace AccessLogWorker.Services
{
    public class AccessLogMessageProcessor : IAccessLogMessageProcessor
    {
        private readonly ILogger<AccessLogMessageProcessor> _logger;
        private readonly IAccessLogRepository _accessLogRepository;

        public AccessLogMessageProcessor(ILogger<AccessLogMessageProcessor> logger, IAccessLogRepository accessLogRepository)
        {
            _logger = logger;
            _accessLogRepository = accessLogRepository;
        }

        public async Task ProcessMessageAsync(AccessLogEvent logEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing log event: {Path}", logEvent.Path);

            // Parse UserId from string to int
            int.TryParse(logEvent.UserId, out int userId);

            // Create AccessLog domain entity (same as CreateLogCommand)
            var logEntry = new AccessLog(
                id: 0, // Repository.Add() will auto-generate using CounterService
                log: logEvent.Path ?? "",
                user: userId,
                time: logEvent.TimestampUtc,
                action: logEvent.Method ?? "N/A",
                details: $"Status: {logEvent.StatusCode}, Duration: {logEvent.DurationMs}ms, TraceId: {logEvent.TraceId}, IP: {logEvent.ClientIp}, Agent: {logEvent.UserAgent}",
                source: "RabbitMQ",
                ipAddress: logEvent.ClientIp
            );

            // Use repository.Add() - same as CreateLogCommand via UnitOfWork (no code duplication!)
            await _accessLogRepository.Add(logEntry, cancellationToken);

            _logger.LogInformation("✅ Successfully saved log event to MongoDB with ID: {Id}", logEntry.Id);
        }
    }
}
