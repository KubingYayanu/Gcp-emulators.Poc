using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Microsoft.Extensions.Logging;

namespace Gcp.PubSub.Poc.Infrastructure.PubSub
{
    public class PublisherHandle : IPublisherHandle
    {
        private readonly IPubSubPublisherPool _publisherPool;
        private readonly PublisherClient _publisher;
        private readonly ILogger _logger;
        private volatile bool _disposed;

        public PublisherHandle(
            IPubSubPublisherPool publisherPool,
            PublisherClient publisher,
            string producerId,
            PubSubTaskConfig config,
            ILogger logger)
        {
            _publisherPool = publisherPool;
            _publisher = publisher;
            ProducerId = producerId;
            ProjectId = config.ProjectId;
            TopicId = config.TopicId;
            _logger = logger;
        }

        public string ProducerId { get; }

        public string ProjectId { get; }

        public string TopicId { get; }

        public async Task<string> PublishAsync(
            PubSubPayload payload,
            CancellationToken cancellationToken = default)
        {
            var pubsubMessage = new PubsubMessage
            {
                Data = ByteString.CopyFromUtf8(payload.Message)
            };

            foreach (var attr in payload.Attributes)
            {
                pubsubMessage.Attributes[attr.Key] = attr.Value;
            }

            var messageId = await _publisher.PublishAsync(pubsubMessage);
            return messageId;
        }

        public async Task ShutdownAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed) return;

            try
            {
                _logger.LogInformation(
                    message: "Shutting down publication for producer {ProducerId}, publisher {ProjectId}:{TopicId}",
                    args:
                    [
                        ProducerId,
                        ProjectId,
                        TopicId
                    ]);

                // 停止 publisher
                await _publisher.ShutdownAsync(TimeSpan.FromSeconds(30));

                // 清理 pool 中的資源
                await _publisherPool.RemovePublisherAsync(ProducerId, ProjectId, TopicId);

                _logger.LogInformation(
                    message: "Stopped publication for producer {ProducerId}, publisher {ProjectId}:{TopicId}",
                    args:
                    [
                        ProducerId,
                        ProjectId,
                        TopicId
                    ]);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    exception: ex,
                    message: "Error shutting down publication for producer {ProducerId}",
                    args: ProducerId);
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            await ShutdownAsync();
        }
    }
}