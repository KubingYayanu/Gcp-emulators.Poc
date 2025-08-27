using Gcp.PubSub.Poc.Application.Interfaces.Jobs;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub.Subscriber;
using Gcp.PubSub.Poc.Domain.Enums;
using Gcp.PubSub.Poc.Domain.Queues.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gcp.PubSub.Poc.Application.Services.OneToOne
{
    public class SubscriberOneToOne : IJobService
    {
        private readonly IPubSubSubscriberManager _subscriberManager;
        private readonly WorkerQueueOptions _queueOptions;
        private readonly ILogger<SubscriberOneToOne> _logger;

        public SubscriberOneToOne(
            IPubSubSubscriberManager subscriberManager,
            IOptions<WorkerQueueOptions> queueOptions,
            ILogger<SubscriberOneToOne> logger)
        {
            _subscriberManager = subscriberManager;
            _queueOptions = queueOptions.Value;
            _logger = logger;
        }

        private string SubscriberName => JobType.ToString();

        public JobType JobType => JobType.SubscriberOneToOne;

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                var config = new PubSubTaskConfig(
                    projectId: _queueOptions.SubscriberOneToOne.ProjectId,
                    topicId: _queueOptions.SubscriberOneToOne.TopicId,
                    subscriptionId: _queueOptions.SubscriberOneToOne.SubscriptionId,
                    subscriberAckDeadline: _queueOptions.SubscriberOneToOne.SubscriberAckDeadline);

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