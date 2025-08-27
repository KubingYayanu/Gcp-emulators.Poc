using Gcp.PubSub.Poc.Application.Interfaces.Jobs;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub.Subscriber;
using Gcp.PubSub.Poc.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Gcp.PubSub.Poc.Application.Handlers.OneToOne
{
    public class SubscriberOneToOneStopHandler : IJobStopHandler
    {
        private readonly IPubSubSubscriberManager _subscriberManager;
        private readonly ILogger<SubscriberOneToOneStopHandler> _logger;

        public SubscriberOneToOneStopHandler(
            IPubSubSubscriberManager subscriberManager,
            ILogger<SubscriberOneToOneStopHandler> logger)
        {
            _subscriberManager = subscriberManager;
            _logger = logger;
        }

        private string SubscriberName => JobType.ToString();

        public JobType JobType => JobType.SubscriberOneToOne;

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _subscriberManager.StopSubscriberAsync(
                    subscriberName: SubscriberName,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up SubscriberA job");
            }
        }
    }
}