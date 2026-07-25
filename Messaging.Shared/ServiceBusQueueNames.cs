namespace Messaging.Shared
{
    /// <summary>Central registry of Azure Service Bus queue names used across services.</summary>
    public static class ServiceBusQueueNames
    {
        public const string AccessLog = "app-queue";
        public const string Email     = "email-queue";
    }
}
