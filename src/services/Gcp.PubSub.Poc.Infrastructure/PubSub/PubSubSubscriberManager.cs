using System.Collections.Concurrent;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gcp.PubSub.Poc.Infrastructure.PubSub
{
    public class PubSubSubscriberManager : IPubSubSubscriberManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PubSubSubscriberManager> _logger;
        private readonly ConcurrentDictionary<string, IPubSubSubscriberHandle> _subscribers = new();
        private readonly SemaphoreSlim _lock = new(1, 1);
        private volatile bool _disposed;

        public PubSubSubscriberManager(
            IServiceProvider serviceProvider,
            ILogger<PubSubSubscriberManager> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public int ActiveSubscriberCount => _subscribers.Count;

        public async Task<IPubSubSubscriberHandle> StartSubscriberAsync(
            string subscriberName,
            PubSubTaskConfig config,
            Func<PubSubPayload, CancellationToken, Task> handleMessageAsync,
            CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(PubSubSubscriberManager));

            if (string.IsNullOrEmpty(subscriberName))
                throw new ArgumentException("Subscriber name cannot be null or empty", nameof(subscriberName));

            await _lock.WaitAsync(cancellationToken);
            try
            {
                if (_subscribers.ContainsKey(subscriberName))
                {
                    throw new InvalidOperationException($"Subscriber '{subscriberName}' is already active");
                }

                // 創建新的 Subscriber 實例 (Transient)
                var subscriber = _serviceProvider.GetRequiredService<IPubSubSubscriber>();
                var subscriberHandle = await subscriber.StartAsync(config, handleMessageAsync, cancellationToken);

                _subscribers[subscriberName] = subscriberHandle;

                _logger.LogInformation(
                    message: "Started managed Subscriber Handle '{SubscriberName}' for "
                             + "ProjectId: {ProjectId}, SubscriptionId:{SubscriptionId}",
                    args:
                    [
                        subscriberName,
                        config.ProjectId,
                        config.SubscriptionId
                    ]);

                return subscriberHandle;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task StopSubscriberAsync(
            string subscriberName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(subscriberName)) return;

            if (_subscribers.TryRemove(subscriberName, out var handle))
            {
                try
                {
                    await handle.StopAsync(cancellationToken);
                    _logger.LogInformation(
                        message: "Stopped managed Subscriber Handle '{SubscriberName}'",
                        args: subscriberName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        exception: ex,
                        message: "Error stopping managed Subscriber Handle '{SubscriberName}'",
                        args: subscriberName);
                    throw;
                }
            }
        }

        public async Task StopAllSubscribersAsync(CancellationToken cancellationToken = default)
        {
            var subscriberNames = _subscribers.Keys.ToList();
            var stopTasks = subscriberNames.Select(name => StopSubscriberAsync(name, cancellationToken));

            await Task.WhenAll(stopTasks);

            _logger.LogInformation("Stopped all {Count} managed Subscriber Handle", subscriberNames.Count);
        }

        public IPubSubSubscriberHandle? GetSubscriberHandle(string subscriberName)
        {
            return _subscribers.GetValueOrDefault(subscriberName);
        }

        public IEnumerable<IPubSubSubscriberHandle> GetActiveSubscriberHandles()
        {
            return _subscribers.Values.ToList();
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                await StopAllSubscribersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing Subscriber Handles");
            }
            finally
            {
                _lock.Dispose();
            }
        }
    }
}