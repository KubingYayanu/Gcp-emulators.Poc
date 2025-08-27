using Gcp.PubSub.Poc.Application.Interfaces.Jobs;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub.Subscriber;
using Gcp.PubSub.Poc.Domain.Enums;
using Gcp.PubSub.Poc.Domain.Queues.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gcp.PubSub.Poc.Application.Services.FilterByAttribute
{
    public class SubscriberFilterByAttributeCat : IJobService
    {
        private readonly IPubSubSubscriberManager _subscriberManager;
        private readonly WorkerQueueOptions _queueOptions;
        private readonly ILogger<SubscriberFilterByAttributeCat> _logger;

        public SubscriberFilterByAttributeCat(
            IPubSubSubscriberManager subscriberManager,
            IOptions<WorkerQueueOptions> queueOptions,
            ILogger<SubscriberFilterByAttributeCat> logger)
        {
            _subscriberManager = subscriberManager;
            _queueOptions = queueOptions.Value;
            _logger = logger;
        }

        private string SubscriberName => JobType.ToString();

        public JobType JobType => JobType.SubscriberFilterByAttributeCat;

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                var config = new PubSubTaskConfig(
                    projectId: _queueOptions.SubscriberFilterByAttributeCat.ProjectId,
                    topicId: _queueOptions.SubscriberFilterByAttributeCat.TopicId,
                    subscriptionId: _queueOptions.SubscriberFilterByAttributeCat.SubscriptionId,
                    subscriberAckDeadline: _queueOptions.SubscriberFilterByAttributeCat.SubscriberAckDeadline);

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
            var partitionKey = envelope.GetExtraAttribute("partition_key", "N/A");

            _logger.LogInformation(
                message: "[{Subscriber}] Processing: {Message}, MessageId: {MessageId}, PartitionKey: {PartitionKey}",
                args: [SubscriberName, envelope.Data, envelope.MessageId, partitionKey]);
            await Task.Delay(100, cancellationToken);
        }
    }
}