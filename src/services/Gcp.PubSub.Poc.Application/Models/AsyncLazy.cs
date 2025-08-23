namespace Gcp.PubSub.Poc.Application.Models
{
    public class AsyncLazy<T>
    {
        private readonly Lazy<Task<T>> _lazy;

        public AsyncLazy(Func<Task<T>> taskFactory)
        {
            _lazy = new Lazy<Task<T>>(taskFactory);
        }

        public bool IsValueCreated => _lazy.IsValueCreated;

        public async Task<T> GetValueAsync() => await _lazy.Value;
    }
}