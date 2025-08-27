using Gcp.PubSub.Poc.Application.Interfaces.Jobs;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub.Subscriber;
using Gcp.PubSub.Poc.Domain.Enums;
using Gcp.PubSub.Poc.Domain.Queues.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gcp.PubSub.Poc.Application.Services.OneToMany
{
    public class SubscriberOneToMany2 : IJobService
    {
        private readonly IPubSubSubscriberManager _subscriberManager;
        private readonly WorkerQueueOptions _queueOptions;
        private readonly ILogger<SubscriberOneToMany2> _logger;

        public SubscriberOneToMany2(
            IPubSubSubscriberManager subscriberManager,
            IOptions<WorkerQueueOptions> queueOptions,
            ILogger<SubscriberOneToMany2> logger)
        {
            _subscriberManager = subscriberManager;
            _queueOptions = queueOptions.Value;
            _logger = logger;
        }

        private string SubscriberName => JobType.ToString();

        public JobType JobType => JobType.SubscriberOneToMany2;

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                var config = new PubSubTaskConfig(
                    projectId: _queueOptions.SubscriberOneToMany2.ProjectId,
                    topicId: _queueOptions.SubscriberOneToMany2.TopicId,
                    subscriptionId: _queueOptions.SubscriberOneToMany2.SubscriptionId,
                    subscriberAckDeadline: _queueOptions.SubscriberOneToMany2.SubscriberAckDeadline);

                await _subscriberManager.StartSubscriberAsync<string>(
                    subscriberName: SubscriberName,
                    config: config,
                    messageHandler: ProcessMessage,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while running {SubscriberName} job", SubscriberName);
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