using Gcp.PubSub.Poc.Application.Interfaces.Jobs;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub.Publisher;
using Gcp.PubSub.Poc.Domain.Enums;
using Gcp.PubSub.Poc.Domain.Queues.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gcp.PubSub.Poc.Application.Services.FilterByAttribute
{
    public class PublisherFilterByAttribute : IJobService
    {
        private readonly IPubSubPublisherManager _publisherManager;
        private readonly WorkerQueueOptions _queueOptions;
        private readonly ILogger<PublisherFilterByAttribute> _logger;

        public PublisherFilterByAttribute(
            IPubSubPublisherManager publisherManager,
            IOptions<WorkerQueueOptions> queueOptions,
            ILogger<PublisherFilterByAttribute> logger)
        {
            _publisherManager = publisherManager;
            _queueOptions = queueOptions.Value;
            _logger = logger;
        }

        private string PublisherName => JobType.ToString();

        public JobType JobType => JobType.PublisherFilterByAttribute;

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                var config = new PubSubTaskConfig(
                    projectId: _queueOptions.PublisherFilterByAttribute.ProjectId,
                    topicId: _queueOptions.PublisherFilterByAttribute.TopicId,
                    subscriptionId: _queueOptions.PublisherFilterByAttribute.SubscriptionId);

                var publisherHandle = await _publisherManager.StartPublisherAsync(
                    publisherName: PublisherName,
                    config: config,
                    cancellationToken: cancellationToken);

                for (int i = 0; i < 20; i++)
                {
                    var message = i.ToString();
                    var partitionKey = i % 2 == 0
                        ? "dog"
                        : "cat";
                    var extraAttributes = new Dictionary<string, string>
                    {
                        { "source", "worker" },
                        { "partition_key", partitionKey }
                    };
                    var envelope = new PubSubEnvelope<string>(
                        data: message,
                        eventType: "filter-by-attribute.published",
                        schemaVersion: "v1",
                        extraAttributes: extraAttributes);

                    var messageId = await publisherHandle.PublishAsync(
                        message: envelope.ToPubsubMessage());

                    envelope.MessageId = messageId;
                    
                    _logger.LogInformation(
                        message: "Published message: {Message}, MessageId: {MessageId}, PartitionKey: {PartitionKey}",
                        args: [message, messageId, partitionKey]);

                    await Task.Delay(100, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while running {PublisherName} job", PublisherName);
            }
        }
    }
}