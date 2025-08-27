using Gcp.PubSub.Poc.Application.Interfaces.Jobs;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub.Publisher;
using Gcp.PubSub.Poc.Domain.Enums;
using Gcp.PubSub.Poc.Domain.Queues.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gcp.PubSub.Poc.Application.Services
{
    public class PublisherA : IJobService
    {
        private readonly IPubSubPublisherManager _publisherManager;
        private readonly WorkerQueueOptions _queueOptions;
        private readonly ILogger<PublisherA> _logger;

        public PublisherA(
            IPubSubPublisherManager publisherManager,
            IOptions<WorkerQueueOptions> queueOptions,
            ILogger<PublisherA> logger)
        {
            _publisherManager = publisherManager;
            _queueOptions = queueOptions.Value;
            _logger = logger;
        }

        private string PublisherName => JobType.ToString();

        public JobType JobType => JobType.PublisherA;

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                var config = new PubSubTaskConfig(
                    projectId: _queueOptions.PublisherA.ProjectId,
                    topicId: _queueOptions.PublisherA.TopicId,
                    subscriptionId: _queueOptions.PublisherA.SubscriptionId);

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
                            { "source", "worker" },
                            { "timestamp", DateTimeOffset.UtcNow.ToString("o") }
                        };
                        var envelope = new PubSubEnvelope<string>(
                            data: message,
                            eventType: "order.created",
                            schemaVersion: "v1",
                            extraAttributes: extraAttributes);

                        await publisherHandle.PublishAsync(
                            message: envelope.ToPubsubMessage());

                        _logger.LogInformation("Published message: {Message}", message);

                        await Task.Delay(300, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while running PublisherA job");
            }
        }
    }
}