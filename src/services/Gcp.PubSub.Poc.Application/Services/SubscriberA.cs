using Gcp.PubSub.Poc.Application.Interfaces.Jobs;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub.Subscriber;
using Gcp.PubSub.Poc.Domain.Enums;
using Gcp.PubSub.Poc.Domain.Queues.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gcp.PubSub.Poc.Application.Services
{
    public class SubscriberA : IJobService
    {
        private readonly IPubSubSubscriberManager _subscriberManager;
        private readonly WorkerQueueOptions _queueOptions;
        private readonly ILogger<SubscriberA> _logger;

        public SubscriberA(
            IPubSubSubscriberManager subscriberManager,
            IOptions<WorkerQueueOptions> queueOptions,
            ILogger<SubscriberA> logger)
        {
            _subscriberManager = subscriberManager;
            _queueOptions = queueOptions.Value;
            _logger = logger;
        }

        private string SubscriberName => nameof(SubscriberA);

        public JobType JobType => JobType.SubscriberA;

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                var config = new PubSubTaskConfig
                {
                    ProjectId = _queueOptions.SubscriberA.ProjectId,
                    TopicId = _queueOptions.SubscriberA.TopicId,
                    SubscriptionId = _queueOptions.SubscriberA.SubscriptionId,
                };

                await _subscriberManager.StartSubscriberAsync(
                    subscriberName: SubscriberName,
                    config: config,
                    handleMessageAsync: (payload, ct) => ProcessMessage(SubscriberName, payload, ct),
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while running SubscriberA job");
            }
            finally
            {
                await _subscriberManager.StopSubscriberAsync(
                    subscriberName: SubscriberName,
                    cancellationToken: cancellationToken);
            }
        }

        private async Task ProcessMessage(
            string subscriberName,
            PubSubPayload payload,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Subscriber}] Processing: {Message}", subscriberName, payload.Message);
            await Task.Delay(100, cancellationToken);
        }
    }
}