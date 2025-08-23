using System.Collections.Concurrent;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gcp.PubSub.Poc.Infrastructure.PubSub
{
    public class PubSubPublisherPool : IPubSubPublisherPool
    {
        private readonly ConcurrentDictionary<string, PublisherClient> _publishers = new();
        private readonly ConcurrentDictionary<string, ProducerRegistration> _registrations = new();
        private readonly SemaphoreSlim _lock;
        private readonly PubSubOptions _options;
        private readonly ILogger<PubSubPublisherPool> _logger;
        private readonly int _maxSize = 1;
        private bool _disposed;

        public PubSubPublisherPool(
            IOptions<PubSubOptions> options,
            ILogger<PubSubPublisherPool> logger)
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

        public async Task<PublisherClient> GetOrCreatePublisherAsync(
            string producerId,
            string projectId,
            string topicId)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(PubSubPublisherPool));

            var publisherKey = $"{producerId}:{projectId}:{topicId}";
            var registrationKey = publisherKey;

            await _lock.WaitAsync();
            try
            {
                // 檢查是否已經註冊
                if (_registrations.ContainsKey(registrationKey))
                {
                    throw new InvalidOperationException(
                        $"Producer {producerId} already registered for {projectId}:{topicId}");
                }

                // 為每個 Producer 創建獨立的 Publisher
                if (!_publishers.TryGetValue(publisherKey, out var publisher))
                {
                    var topicName = TopicName.FromProjectTopic(projectId, topicId);
                    var builder = new PublisherClientBuilder
                    {
                        Endpoint = Endpoint,
                        TopicName = topicName,
                        EmulatorDetection = EmulatorDetection
                    };

                    publisher = await builder.BuildAsync();
                    _publishers[publisherKey] = publisher;
                }

                // 註冊 Producer
                _registrations[registrationKey] = new ProducerRegistration
                {
                    ProducerId = producerId,
                    RegisteredAt = DateTime.UtcNow
                };

                return publisher;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task RemovePublisherAsync(
            string producerId,
            string projectId,
            string topicId)
        {
            var publisherKey = $"{producerId}:{projectId}:{topicId}";
            var registrationKey = publisherKey;

            await _lock.WaitAsync();
            try
            {
                // 移除註冊
                _registrations.TryRemove(registrationKey, out _);

                // 停止並移除 Publisher
                if (_publishers.TryRemove(publisherKey, out var publisher))
                {
                    try
                    {
                        await publisher.ShutdownAsync(TimeSpan.FromSeconds(30));
                        await publisher.DisposeAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error stopping publisher for {PublisherKey}", publisherKey);
                    }
                }
            }
            finally
            {
                _lock.Release();
            }
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
                foreach (var publisher in _publishers.Values)
                {
                    try
                    {
                        await publisher.ShutdownAsync(TimeSpan.FromSeconds(10));
                        await publisher.DisposeAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error disposing publisher");
                    }
                }

                _publishers.Clear();
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