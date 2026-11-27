namespace MyWarehouse.Infrastructure.Services
{
    /// <summary>
    /// Lets EmailQueueService wake the outbox processor right after a row is saved,
    /// so emails go out immediately instead of waiting for the next polling pass.
    /// </summary>
    public interface IEmailOutboxSignal
    {
        void Notify();
        Task WaitAsync(TimeSpan timeout, CancellationToken cancellationToken);
    }

    public sealed class EmailOutboxSignal : IEmailOutboxSignal
    {
        private readonly SemaphoreSlim _semaphore = new(0, int.MaxValue);

        public void Notify()
        {
            try { _semaphore.Release(); }
            catch (SemaphoreFullException) { /* already signalled many times; nothing to do */ }
        }

        public async Task WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(timeout, cancellationToken);
        }
    }
}
