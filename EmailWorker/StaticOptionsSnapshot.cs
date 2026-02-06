using Microsoft.Extensions.Options;


namespace EmailWorker
{
    public sealed class StaticOptionsSnapshot<T> : IOptionsSnapshot<T> where T : class, new()
    {
        public StaticOptionsSnapshot(T value) => Value = value;

        public T Value { get; }

        public T Get(string? name) => Value;
    }
}
