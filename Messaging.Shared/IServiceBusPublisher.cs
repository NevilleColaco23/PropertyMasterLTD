namespace Messaging.Shared
{
    public interface IServiceBusPublisher
    {
        Task SendAsync<T>(T message, string queueName, CancellationToken cancellationToken = default);
    }
}
