using System.Collections.Concurrent;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gcp.PubSub.Poc.Infrastructure.PubSub
{
    public class PubSubPublisherManager : IPubSubPublisherManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PubSubPublisherManager> _logger;
        private readonly ConcurrentDictionary<string, IPublisherHandle> _publishers = new();
        private readonly SemaphoreSlim _lock = new(1, 1);
        private volatile bool _disposed;

        public PubSubPublisherManager(
            IServiceProvider serviceProvider,
            ILogger<PubSubPublisherManager> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public int ActivePublisherCount => _publishers.Count;

        public async Task<IPublisherHandle> StartPublisherAsync(
            string publisherName,
            PubSubTaskConfig config,
            CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(PubSubPublisherManager));

            if (string.IsNullOrEmpty(publisherName))
                throw new ArgumentException("Publisher name cannot be null or empty", nameof(publisherName));

            await _lock.WaitAsync(cancellationToken);
            try
            {
                if (_publishers.ContainsKey(publisherName))
                {
                    throw new InvalidOperationException($"Publisher '{publisherName}' is already active");
                }

                // 創建新的 Producer 實例(Transient)
                var producer = _serviceProvider.GetRequiredService<IPubSubPublisher>();
                var handle = await producer.StartAsync(config, cancellationToken);

                _publishers[publisherName] = handle;

                _logger.LogInformation(
                    message: "Started managed publisher '{PublisherName}' for {ProjectId}:{TopicId}",
                    args:
                    [
                        publisherName,
                        config.ProjectId,
                        config.SubscriptionId
                    ]);

                return handle;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task StopPublisherAsync(
            string publisherName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(publisherName)) return;

            if (_publishers.TryRemove(publisherName, out var handle))
            {
                try
                {
                    await handle.ShutdownAsync(cancellationToken);
                    _logger.LogInformation(
                        message: "Stopped managed publisher '{PublisherName}'",
                        args: publisherName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        exception: ex,
                        message: "Error stopping managed publisher '{PublisherName}'",
                        args: publisherName);
                    throw;
                }
            }
        }

        public async Task StopAllPublishersAsync(CancellationToken cancellationToken = default)
        {
            var publisherNames = _publishers.Keys.ToList();
            var stopTasks = publisherNames.Select(name => StopPublisherAsync(name, cancellationToken));

            await Task.WhenAll(stopTasks);

            _logger.LogInformation("Stopped all {Count} managed publishers", publisherNames.Count);
        }

        public IPublisherHandle? GetPublisherHandle(string publisherName)
        {
            return _publishers.GetValueOrDefault(publisherName);
        }

        public IEnumerable<IPublisherHandle> GetActivePublisherHandles()
        {
            return _publishers.Values.ToList();
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                await StopAllPublishersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during disposal");
            }
            finally
            {
                _lock.Dispose();
            }
        }
    }
}