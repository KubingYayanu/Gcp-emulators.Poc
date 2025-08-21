using System.Collections.Concurrent;
using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Options;

namespace Gcp.PubSub.Poc.Helpers.V2
{
    public class PubSubPublisherPool : IPubSubPublisherPool
    {
        private readonly ConcurrentDictionary<string, AsyncLazy<PublisherClient>> _publishers = new();
        private readonly SemaphoreSlim _lock;
        private readonly PubSubOptions _options;
        private readonly int _maxSize = 10;
        private bool _disposed;

        public PubSubPublisherPool(IOptions<PubSubOptions> options)
        {
            _options = options.Value;
            _lock = new SemaphoreSlim(_maxSize, _maxSize);
        }
        
        private EmulatorDetection EmulatorDetection => _options.Emulated
            ? EmulatorDetection.EmulatorOnly
            : EmulatorDetection.ProductionOnly;

        public async Task<PublisherClient> GetPublisherAsync(string projectId, string topicId)
        {
            var key = $"{projectId}:{topicId}";

            var lazyPublisher = _publishers.GetOrAdd(key,
                _ => new AsyncLazy<PublisherClient>(async () =>
                {
                    var topicName = TopicName.FromProjectTopic(projectId, topicId);
                    var builder = new PublisherClientBuilder
                    {
                        TopicName = topicName,
                        EmulatorDetection = EmulatorDetection
                    };
                    var client = await builder.BuildAsync();
                    
                    return client;
                }));

            return await lazyPublisher.GetValueAsync();
        }

        /// <summary>
        /// 由 Host 自動調用，釋放資源
        /// </summary>
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