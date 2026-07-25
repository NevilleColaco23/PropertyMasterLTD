namespace Messaging.Shared
{
    public class ServiceBusOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string AccessLogQueueName { get; set; } = ServiceBusQueueNames.AccessLog;
        public string EmailQueueName { get; set; } = ServiceBusQueueNames.Email;
    }
}
