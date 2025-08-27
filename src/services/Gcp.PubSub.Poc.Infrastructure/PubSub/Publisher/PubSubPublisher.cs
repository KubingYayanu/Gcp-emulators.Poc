using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub.Publisher;
using Microsoft.Extensions.Logging;

namespace Gcp.PubSub.Poc.Infrastructure.PubSub.Publisher
{
    public class PubSubPublisher : IPubSubPublisher
    {
        private readonly IPubSubPublisherPool _publisherPool;
        private readonly ILogger<PubSubPublisher> _logger;
        private readonly string _publisherId;

        public PubSubPublisher(
            IPubSubPublisherPool publisherPool,
            ILogger<PubSubPublisher> logger)
        {
            _publisherPool = publisherPool;
            _logger = logger;
            _publisherId = Guid.NewGuid().ToString("N")[..8];
        }

        public async Task<IPubSubPublisherHandle> StartAsync(
            PubSubTaskConfig config,
            CancellationToken cancellationToken = default)
        {
            var publisher = await _publisherPool.GetOrCreatePublisherAsync(
                publisherId: _publisherId,
                projectId: config.ProjectId,
                topicId: config.TopicId,
                orderingKey: config.OrderingKey,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                message: "Publisher {PublisherId} started for publication. "
                         + "ProjectId: {ProjectId}, TopicId: {TopicId}",
                args:
                [
                    _publisherId,
                    config.ProjectId,
                    config.TopicId
                ]);

            return new PubSubPublisherHandle(
                publisherId: _publisherId,
                config: config,
                publisherPool: _publisherPool,
                publisher: publisher,
                logger: _logger);
        }
    }
}