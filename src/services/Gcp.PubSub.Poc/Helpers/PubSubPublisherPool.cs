using System.Collections.Concurrent;
using Google.Cloud.PubSub.V1;

namespace Gcp.PubSub.Poc.Helpers
{
    public class PubSubPublisherPool : IPubSubPublisherPool
    {
        private readonly ConcurrentDictionary<string, AsyncLazy<PublisherClient>> _publishers = new();
        private readonly SemaphoreSlim _lock;
        private readonly int _maxSize = 10;
        private bool _disposed;

        public PubSubPublisherPool()
        {
            _lock = new SemaphoreSlim(_maxSize, _maxSize);
        }

        public async Task<PublisherClient> GetPublisherAsync(string projectId, string topicId)
        {
            var key = $"{projectId}:{topicId}";

            var lazyPublisher = _publishers.GetOrAdd(key,
                _ => new AsyncLazy<PublisherClient>(async () =>
                {
                    var topicName = TopicName.FromProjectTopic(projectId, topicId);
                    var publisherClient = await PublisherClient.CreateAsync(topicName);
                    return publisherClient;
                }));

            return await lazyPublisher.GetValueAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            await _lock.WaitAsync();
            try
            {
                foreach (var lazyPublisher in _publishers.Values)
                {
                    if (lazyPublisher.IsValueCreated)
                    {
                        var publisher = await lazyPublisher.GetValueAsync();
                        await publisher.DisposeAsync();
                    }
                }

                _publishers.Clear();
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