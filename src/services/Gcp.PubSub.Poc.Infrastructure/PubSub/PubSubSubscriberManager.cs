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
        private readonly ConcurrentDictionary<string, ISubscriptionHandle> _subscribers = new();
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

        public async Task<ISubscriptionHandle> StartSubscriberAsync(
            string subscriberName,
            PubSubTaskConfig config,
            Func<PubSubPayload, CancellationToken, Task> handleMessageAsync,
            CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(PubSubSubscriberManager));

            if (string.IsNullOrEmpty(subscriberName))
                throw new ArgumentException("Subscription name cannot be null or empty", nameof(subscriberName));

            await _lock.WaitAsync(cancellationToken);
            try
            {
                if (_subscribers.ContainsKey(subscriberName))
                {
                    throw new InvalidOperationException($"Subscription '{subscriberName}' is already active");
                }

                // 創建新的 Consumer 實例(Transient)
                var consumer = _serviceProvider.GetRequiredService<IPubSubSubscriber>();
                var handle = await consumer.StartAsync(config, handleMessageAsync, cancellationToken);

                _subscribers[subscriberName] = handle;

                _logger.LogInformation(
                    "Started managed subscription '{SubscriptionName}' for {ProjectId}:{SubscriptionId}",
                    subscriberName,
                    config.ProjectId,
                    config.SubscriptionId);

                return handle;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task StopSubscriptionAsync(string subscriptionName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(subscriptionName)) return;

            if (_subscribers.TryRemove(subscriptionName, out var handle))
            {
                try
                {
                    await handle.StopAsync(cancellationToken);
                    _logger.LogInformation("Stopped managed subscription '{SubscriptionName}'", subscriptionName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error stopping managed subscription '{SubscriptionName}'", subscriptionName);
                    throw;
                }
            }
        }

        public async Task StopAllSubscriptionsAsync(CancellationToken cancellationToken = default)
        {
            var subscriptionNames = _subscribers.Keys.ToList();
            var stopTasks = subscriptionNames.Select(name => StopSubscriptionAsync(name, cancellationToken));

            await Task.WhenAll(stopTasks);

            _logger.LogInformation("Stopped all {Count} managed subscriptions", subscriptionNames.Count);
        }

        public ISubscriptionHandle? GetSubscription(string subscriptionName)
        {
            return _subscribers.GetValueOrDefault(subscriptionName);
        }

        public IEnumerable<ISubscriptionHandle> GetActiveSubscriptions()
        {
            return _subscribers.Values.ToList();
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                await StopAllSubscriptionsAsync();
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