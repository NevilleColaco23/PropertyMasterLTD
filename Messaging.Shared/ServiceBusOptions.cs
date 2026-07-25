namespace Messaging.Shared
{
    public class ServiceBusOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string AccessLogQueueName { get; set; } = "accesslog-queue";
    }
}
