using System.Collections.Concurrent;
using Google.Api.Gax;
using Google.Cloud.PubSub.V1;

namespace Gcp.PubSub.Poc.Helpers
{
    public class PubSubSubscriberPool : IPubSubSubscriberPool
    {
        private readonly ConcurrentDictionary<string, AsyncLazy<SubscriberClient>> _subscribers = new();
        private readonly SemaphoreSlim _lock;
        private readonly int _maxSize = 10;
        private bool _disposed;

        public PubSubSubscriberPool()
        {
            _lock = new SemaphoreSlim(_maxSize, _maxSize);
        }

        public async Task<SubscriberClient> GetSubscriberAsync(string projectId, string subscriptionId)
        {
            var key = $"{projectId}:{subscriptionId}";

            var lazySubscriber = _subscribers.GetOrAdd(key,
                _ => new AsyncLazy<SubscriberClient>(async () =>
                {
                    var subscriptionName = SubscriptionName
                        .FromProjectSubscription(projectId, subscriptionId);
                    var settings = new SubscriberClient.Settings
                    {
                        AckDeadline = TimeSpan.FromSeconds(60)
                    };
                    var builder = new SubscriberClientBuilder
                    {
                        SubscriptionName = subscriptionName,
                        Settings = settings,
                        EmulatorDetection = EmulatorDetection.EmulatorOnly
                    };

                    var client = await builder.BuildAsync();
                    return client;
                }));

            return await lazySubscriber.GetValueAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            await _lock.WaitAsync();
            try
            {
                foreach (var lazySubscriber in _subscribers.Values)
                {
                    if (lazySubscriber.IsValueCreated)
                    {
                        var subscriber = await lazySubscriber.GetValueAsync();
                        await subscriber.DisposeAsync();
                    }
                }

                _subscribers.Clear();
                _disposed = true;
            }
            finally
            {
                _lock.Release();
                _lock.Dispose();
            }
        }
    }
}