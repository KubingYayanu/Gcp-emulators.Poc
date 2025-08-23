using System.Collections.Concurrent;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gcp.PubSub.Poc.Infrastructure.PubSub
{
    public class PubSubSubscriberPool : IPubSubSubscriberPool
    {
        private readonly ConcurrentDictionary<string, SubscriberClient> _subscribers = new();
        private readonly ConcurrentDictionary<string, ConsumerRegistration> _registrations = new();
        private readonly SemaphoreSlim _lock;
        private readonly PubSubOptions _options;
        private readonly ILogger<PubSubSubscriberPool> _logger;
        private readonly int _maxSize = 1;
        private bool _disposed;

        public PubSubSubscriberPool(
            IOptions<PubSubOptions> options,
            ILogger<PubSubSubscriberPool> logger)
        {
            _options = options.Value;
            _logger = logger;
            _lock = new SemaphoreSlim(_maxSize, _maxSize);
        }

        private EmulatorDetection EmulatorDetection => _options.Emulated
            ? EmulatorDetection.EmulatorOnly
            : EmulatorDetection.ProductionOnly;

        private string? Endpoint => EmulatorDetection == EmulatorDetection.EmulatorOnly
            ? null
            : _options.Endpoint;

        public async Task<SubscriberClient> GetOrCreateSubscriberAsync(
            string consumerId,
            string projectId,
            string subscriptionId,
            Func<PubSubPayload, CancellationToken, Task> handler)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(PubSubSubscriberPool));

            var subscriberKey = $"{consumerId}:{projectId}:{subscriptionId}";
            var registrationKey = subscriberKey;

            await _lock.WaitAsync();
            try
            {
                // 檢查是否已經註冊
                if (_registrations.ContainsKey(registrationKey))
                {
                    throw new InvalidOperationException(
                        $"Consumer {consumerId} already registered for {projectId}:{subscriptionId}");
                }

                // 為每個 Consumer 創建獨立的 Subscriber
                if (!_subscribers.TryGetValue(subscriberKey, out var subscriber))
                {
                    var subscriptionName = SubscriptionName
                        .FromProjectSubscription(projectId, subscriptionId);

                    var settings = new SubscriberClient.Settings
                    {
                        // TODO: 改為參數傳入
                        AckDeadline = TimeSpan.FromSeconds(60)
                    };

                    var builder = new SubscriberClientBuilder
                    {
                        Endpoint = Endpoint,
                        SubscriptionName = subscriptionName,
                        Settings = settings,
                        EmulatorDetection = EmulatorDetection
                    };

                    subscriber = await builder.BuildAsync();
                    _subscribers[subscriberKey] = subscriber;
                }

                // 註冊 Consumer
                _registrations[registrationKey] = new ConsumerRegistration
                {
                    ConsumerId = consumerId,
                    Handler = handler,
                    RegisteredAt = DateTime.UtcNow
                };

                return subscriber;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task RemoveSubscriberAsync(string consumerId, string projectId, string subscriptionId)
        {
            var subscriberKey = $"{consumerId}:{projectId}:{subscriptionId}";
            var registrationKey = subscriberKey;

            await _lock.WaitAsync();
            try
            {
                // 移除註冊
                _registrations.TryRemove(registrationKey, out _);

                // 停止並移除 Subscriber
                if (_subscribers.TryRemove(subscriberKey, out var subscriber))
                {
                    try
                    {
                        await subscriber.StopAsync(TimeSpan.FromSeconds(30));
                        await subscriber.DisposeAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error stopping subscriber for {SubscriberKey}", subscriberKey);
                    }
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Host 自動調用，釋放資源
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            await _lock.WaitAsync();
            try
            {
                foreach (var subscriber in _subscribers.Values)
                {
                    try
                    {
                        await subscriber.StopAsync(TimeSpan.FromSeconds(10));
                        await subscriber.DisposeAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error disposing subscriber");
                    }
                }

                _subscribers.Clear();
                _registrations.Clear();
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