using Gcp.PubSub.Poc.Application.Interfaces.Jobs;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub.Publisher;
using Gcp.PubSub.Poc.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Gcp.PubSub.Poc.Application.Handlers
{
    public class PublisherAStopHandler : IJobStopHandler
    {
        private readonly IPubSubPublisherManager _publisherManager;
        private readonly ILogger<PublisherAStopHandler> _logger;

        public PublisherAStopHandler(
            IPubSubPublisherManager publisherManager,
            ILogger<PublisherAStopHandler> logger)
        {
            _publisherManager = publisherManager;
            _logger = logger;
        }

        private string PublisherName => JobType.ToString();

        public JobType JobType => JobType.PublisherA;

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _publisherManager.StopPublisherAsync(
                    publisherName: PublisherName,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up PublisherA job");
            }
        }
    }
}