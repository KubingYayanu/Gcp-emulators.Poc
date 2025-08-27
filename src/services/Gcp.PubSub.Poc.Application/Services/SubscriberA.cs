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

        private string SubscriberName => JobType.ToString();

        public JobType JobType => JobType.SubscriberA;

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                var config = new PubSubTaskConfig(
                    projectId: _queueOptions.SubscriberA.ProjectId,
                    topicId: _queueOptions.SubscriberA.TopicId,
                    subscriptionId: _queueOptions.SubscriberA.SubscriptionId,
                    subscriberAckDeadline: _queueOptions.SubscriberA.SubscriberAckDeadline);

                await _subscriberManager.StartSubscriberAsync<string>(
                    subscriberName: SubscriberName,
                    config: config,
                    messageHandler: ProcessMessage,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while running SubscriberA job");
            }
        }

        private async Task ProcessMessage(
            PubSubEnvelope<string> envelope,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                message: "[{Subscriber}] Processing: {Message}, MessageId: {MessageId}",
                args: [SubscriberName, envelope.Data, envelope.MessageId]);
            await Task.Delay(100, cancellationToken);
        }
    }
}