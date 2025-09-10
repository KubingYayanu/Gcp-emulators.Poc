using Gcp.PubSub.Poc.Application.Interfaces.Jobs;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub.Publisher;
using Gcp.PubSub.Poc.Domain.Enums;
using Gcp.PubSub.Poc.Domain.Queues.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gcp.PubSub.Poc.Application.Services.OneToOne
{
    public class PublisherOneToOne : IJobService
    {
        private readonly IPubSubPublisherManager _publisherManager;
        private readonly WorkerQueueOptions _queueOptions;
        private readonly ILogger<PublisherOneToOne> _logger;

        public PublisherOneToOne(
            IPubSubPublisherManager publisherManager,
            IOptions<WorkerQueueOptions> queueOptions,
            ILogger<PublisherOneToOne> logger)
        {
            _publisherManager = publisherManager;
            _queueOptions = queueOptions.Value;
            _logger = logger;
        }

        private string PublisherName => JobType.ToString();

        public JobType JobType => JobType.PublisherOneToOne;

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                var config = new PubSubTaskConfig(
                    projectId: _queueOptions.PublisherOneToOne.ProjectId,
                    topicId: _queueOptions.PublisherOneToOne.TopicId,
                    subscriptionId: _queueOptions.PublisherOneToOne.SubscriptionId);

                var publisherHandle = await _publisherManager.StartPublisherAsync(
                    publisherName: PublisherName,
                    config: config,
                    cancellationToken: cancellationToken);

                while (!cancellationToken.IsCancellationRequested)
                {
                    if (Console.KeyAvailable)
                    {
                        var message = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(message))
                        {
                            continue;
                        }
                        
                        var extraAttributes = new Dictionary<string, string>
                        {
                            { "source", "worker" }
                        };
                        var envelope = new PubSubEnvelope<string>(
                            data: message,
                            eventType: "one-to-one.published",
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while running {PublisherName} job", PublisherName);
            }
        }
    }
}