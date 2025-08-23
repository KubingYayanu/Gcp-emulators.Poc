using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Microsoft.Extensions.Logging;

namespace Gcp.PubSub.Poc.Infrastructure.PubSub
{
    public class PubSubPublisher : IPubSubPublisher
    {
        private readonly IPubSubPublisherPool _publisherPool;
        private readonly ILogger<PubSubPublisher> _logger;
        private readonly string _producerId;

        public PubSubPublisher(
            IPubSubPublisherPool publisherPool,
            ILogger<PubSubPublisher> logger)
        {
            _publisherPool = publisherPool;
            _logger = logger;
            _producerId = Guid.NewGuid().ToString("N")[..8];
        }

        public async Task<IPublisherHandle> StartAsync(
            PubSubTaskConfig config,
            CancellationToken cancellationToken = default)
        {
            var publisher = await _publisherPool.GetOrCreatePublisherAsync(
                producerId: _producerId,
                projectId: config.ProjectId,
                topicId: config.TopicId);

            _logger.LogInformation(
                message: "Producer {ProducerId} started for publication {ProjectId}:{TopicId}",
                args:
                [
                    _producerId,
                    config.ProjectId,
                    config.TopicId
                ]);

            return new PublisherHandle(
                publisherPool: _publisherPool,
                publisher: publisher,
                producerId: _producerId,
                config: config,
                logger: _logger);
        }
    }
}