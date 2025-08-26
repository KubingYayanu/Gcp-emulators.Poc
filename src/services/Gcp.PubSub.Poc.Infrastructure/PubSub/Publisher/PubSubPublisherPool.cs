using System.Collections.Concurrent;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub.Publisher;
using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gcp.PubSub.Poc.Infrastructure.PubSub.Publisher
{
    public class PubSubPublisherPool : IPubSubPublisherPool
    {
        private readonly ConcurrentDictionary<string, PublisherClient> _publishers = new();
        private readonly ConcurrentDictionary<string, PublisherRegistration> _registrations = new();
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
            : _options.Host;

        public async Task<PublisherClient> GetOrCreatePublisherAsync(
            string publisherId,
            string projectId,
            string topicId,
            CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(PubSubPublisherPool));

            var publisherKey = $"{publisherId}:{projectId}:{topicId}";
            var registrationKey = publisherKey;

            await _lock.WaitAsync(cancellationToken);
            try
            {
                // 檢查是否已經註冊
                if (_registrations.ContainsKey(registrationKey))
                {
                    throw new InvalidOperationException(
                        $"Publisher {publisherId} already registered for ProjectId: {projectId}, TopicId: {topicId}");
                }

                // 為每個 PublisherKey 創建獨立的 Publisher
                if (!_publishers.TryGetValue(publisherKey, out var publisher))
                {
                    var topicName = TopicName.FromProjectTopic(projectId, topicId);
                    var builder = new PublisherClientBuilder
                    {
                        Endpoint = Endpoint,
                        TopicName = topicName,
                        EmulatorDetection = EmulatorDetection
                    };

                    publisher = await builder.BuildAsync(cancellationToken);
                    _publishers[publisherKey] = publisher;
                }

                // 註冊 Publisher
                _registrations[registrationKey] = new PublisherRegistration
                {
                    PublisherId = publisherId,
                    RegisteredAt = DateTimeOffset.UtcNow
                };

                return publisher;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task RemovePublisherAsync(
            string publisherId,
            string projectId,
            string topicId,
            CancellationToken cancellationToken = default)
        {
            var publisherKey = $"{publisherId}:{projectId}:{topicId}";
            var registrationKey = publisherKey;

            await _lock.WaitAsync(cancellationToken);
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
                        _logger.LogError(
                            exception: ex,
                            message: "Error stopping publisher. PublisherId: {PublisherId}, "
                                     + "ProjectId: {ProjectId}, TopicId: {TopicId}",
                            args: [publisherId, projectId, topicId]);
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
                if (_disposed) return;

                _disposed = true;

                // 清理所有 Publishers
                var publishers = _publishers.Values.ToList();
                _publishers.Clear();
                _registrations.Clear();

                var disposeTasks = publishers.Select(async publisher =>
                {
                    try
                    {
                        await publisher.ShutdownAsync(TimeSpan.FromSeconds(10));
                        await publisher.DisposeAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            exception: ex,
                            message: "Error disposing publisher");
                    }
                });

                await Task.WhenAll(disposeTasks);
            }
            finally
            {
                _lock.Release();
                _lock.Dispose();
            }
        }
    }
}