using Gcp.PubSub.Poc.Application.Interfaces.Jobs;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub.Publisher;
using Gcp.PubSub.Poc.Domain.Enums;
using Gcp.PubSub.Poc.Domain.Queues.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gcp.PubSub.Poc.Application.Services.OneToMany
{
    public class PublisherOneToMany : IJobService
    {
        private readonly IPubSubPublisherManager _publisherManager;
        private readonly WorkerQueueOptions _queueOptions;
        private readonly ILogger<PublisherOneToMany> _logger;

        public PublisherOneToMany(
            IPubSubPublisherManager publisherManager,
            IOptions<WorkerQueueOptions> queueOptions,
            ILogger<PublisherOneToMany> logger)
        {
            _publisherManager = publisherManager;
            _queueOptions = queueOptions.Value;
            _logger = logger;
        }

        private string PublisherName => JobType.ToString();

        public JobType JobType => JobType.PublisherOneToMany;

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                var config = new PubSubTaskConfig(
                    projectId: _queueOptions.PublisherOneToMany.ProjectId,
                    topicId: _queueOptions.PublisherOneToMany.TopicId,
                    subscriptionId: _queueOptions.PublisherOneToMany.SubscriptionId);

                var publisherHandle = await _publisherManager.StartPublisherAsync(
                    publisherName: PublisherName,
                    config: config,
                    cancellationToken: cancellationToken);

                for (int i = 0; i < 100; i++)
                {
                    var message = i.ToString();
                    var extraAttributes = new Dictionary<string, string>
                    {
                        { "source", "worker" }
                    };
                    var envelope = new PubSubEnvelope<string>(
                        data: message,
                        eventType: "ono-to-many.published",
                        schemaVersion: "v1",
                        extraAttributes: extraAttributes);

                    var messageId = await publisherHandle.PublishAsync(
                        message: envelope.ToPubsubMessage());
                    
                    envelope.MessageId = messageId;

                    _logger.LogInformation(
                        message: "Published message: {Message}, MessageId: {MessageId}",
                        args: [message, messageId]);

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