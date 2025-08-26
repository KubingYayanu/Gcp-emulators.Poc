using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub.Publisher;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;

namespace Gcp.PubSub.Poc.Infrastructure.PubSub.Publisher
{
    public class PubSubPublisherHandle : IPubSubPublisherHandle
    {
        private readonly IPubSubPublisherPool _publisherPool;
        private readonly PublisherClient _publisher;
        private readonly ILogger _logger;
        private volatile bool _disposed;

        public PubSubPublisherHandle(
            string publisherId,
            PubSubTaskConfig config,
            IPubSubPublisherPool publisherPool,
            PublisherClient publisher,
            ILogger logger)
        {
            PublisherId = publisherId;
            ProjectId = config.ProjectId;
            TopicId = config.TopicId;
            _publisherPool = publisherPool;
            _publisher = publisher;
            _logger = logger;
        }

        public string PublisherId { get; }

        public string ProjectId { get; }

        public string TopicId { get; }

        public async Task<string> PublishAsync(PubsubMessage payload)
        {
            var messageId = await _publisher.PublishAsync(payload);
            return messageId;
        }

        public async Task ShutdownAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed) return;

            try
            {
                _logger.LogInformation(
                    message: "Shutting down publication for PublisherId: {PublisherId}, "
                             + "ProjectId: {ProjectId}, TopicId: {TopicId}",
                    args:
                    [
                        PublisherId,
                        ProjectId,
                        TopicId
                    ]);

                // 停止 publisher
                await _publisher.ShutdownAsync(TimeSpan.FromSeconds(30));

                // 清理 pool 中的資源
                await _publisherPool.RemovePublisherAsync(
                    publisherId: PublisherId,
                    projectId: ProjectId,
                    topicId: TopicId,
                    cancellationToken: cancellationToken);

                _logger.LogInformation(
                    message: "Stopped publication for PublisherId: {PublisherId}, "
                             + "ProjectId: {ProjectId}, TopicId: {TopicId}",
                    args:
                    [
                        PublisherId,
                        ProjectId,
                        TopicId
                    ]);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    exception: ex,
                    message: "Error shutting down publication for PublisherId: {ProducerId}"
                             + "ProjectId: {ProjectId}, TopicId: {TopicId}",
                    args:
                    [
                        PublisherId,
                        ProjectId,
                        TopicId
                    ]);
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