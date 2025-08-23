using System.Collections.Concurrent;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gcp.PubSub.Poc.Infrastructure.PubSub
{
    public class PubSubSubscriptionManager : IPubSubSubscriptionManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PubSubSubscriptionManager> _logger;
        private readonly ConcurrentDictionary<string, ISubscriptionHandle> _subscriptions = new();
        private readonly SemaphoreSlim _lock = new(1, 1);
        private volatile bool _disposed;

        public PubSubSubscriptionManager(
            IServiceProvider serviceProvider,
            ILogger<PubSubSubscriptionManager> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public int ActiveSubscriptionCount => _subscriptions.Count;

        public async Task<ISubscriptionHandle> StartSubscriptionAsync(
            string subscriptionName,
            PubSubTaskConfig config,
            Func<PubSubPayload, CancellationToken, Task> handleMessageAsync,
            CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(PubSubSubscriptionManager));

            if (string.IsNullOrEmpty(subscriptionName))
                throw new ArgumentException("Subscription name cannot be null or empty", nameof(subscriptionName));

            await _lock.WaitAsync(cancellationToken);
            try
            {
                if (_subscriptions.ContainsKey(subscriptionName))
                {
                    throw new InvalidOperationException($"Subscription '{subscriptionName}' is already active");
                }

                // 創建新的 Consumer 實例（Transient）
                var consumer = _serviceProvider.GetRequiredService<IPubSubSubscriber>();
                var handle = await consumer.StartAsync(config, handleMessageAsync, cancellationToken);

                _subscriptions[subscriptionName] = handle;

                _logger.LogInformation(
                    "Started managed subscription '{SubscriptionName}' for {ProjectId}:{SubscriptionId}",
                    subscriptionName,
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

            if (_subscriptions.TryRemove(subscriptionName, out var handle))
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
            var subscriptionNames = _subscriptions.Keys.ToList();
            var stopTasks = subscriptionNames.Select(name => StopSubscriptionAsync(name, cancellationToken));

            await Task.WhenAll(stopTasks);

            _logger.LogInformation("Stopped all {Count} managed subscriptions", subscriptionNames.Count);
        }

        public ISubscriptionHandle? GetSubscription(string subscriptionName)
        {
            return _subscriptions.GetValueOrDefault(subscriptionName);
        }

        public IEnumerable<ISubscriptionHandle> GetActiveSubscriptions()
        {
            return _subscriptions.Values.ToList();
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