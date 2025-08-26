using Gcp.PubSub.Poc.Domain.Queues.Options;
using Gcp.PubSub.Poc.Helpers;
using Gcp.PubSub.Poc.Helpers.V2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gcp.PubSub.Poc.Producer.Services
{
    public class ProducerService : IProducerService
    {
        private readonly IPubSubPublisher _pubSubPublisher;
        private readonly WorkerQueueOptions _options;
        private readonly ILogger<ProducerService> _logger;

        public ProducerService(
            IPubSubPublisher pubSubPublisher,
            IOptions<WorkerQueueOptions> options,
            ILogger<ProducerService> logger)
        {
            _pubSubPublisher = pubSubPublisher;
            _options = options.Value;
            _logger = logger;
        }

        public async Task PublishMessagesAsync(CancellationToken cancellationToken = default)
        {
            var config = new PubSubTaskConfig
            {
                ProjectId = _options.PublisherA.ProjectId,
                TopicId = _options.PublisherA.TopicId,
                SubscriptionId = _options.PublisherA.SubscriptionId,
            };

            while (!cancellationToken.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    var message = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        continue;
                    }

                    await _pubSubPublisher.PublishAsync(
                        config: config,
                        payload: new PubSubPayload
                        {
                            Message = message,
                            Attributes = new Dictionary<string, string>
                            {
                                { "timestamp", DateTime.UtcNow.ToString("o") },
                                { "source", "console" }
                            }
                        },
                        cancellationToken: cancellationToken);

                    _logger.LogInformation("Published message: {Message}", message);

                    await Task.Delay(300, cancellationToken);
                }
            }
        }
    }
}