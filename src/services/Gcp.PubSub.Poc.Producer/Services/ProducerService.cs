using Gcp.PubSub.Poc.Helpers;
using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gcp.PubSub.Poc.Producer.Services
{
    public class ProducerService : IProducerService
    {
        private readonly IPubSubResourceHelper _pubSubResourceHelper;
        private readonly PubSubOptions _options;
        private readonly ILogger<ProducerService> _logger;

        public ProducerService(
            IPubSubResourceHelper pubSubResourceHelper,
            IOptions<PubSubOptions> options,
            ILogger<ProducerService> logger)
        {
            _pubSubResourceHelper = pubSubResourceHelper;
            _options = options.Value;
            _logger = logger;
        }

        private EmulatorDetection EmulatorDetection => _options.Emulated
            ? EmulatorDetection.EmulatorOnly
            : EmulatorDetection.ProductionOnly;

        private async Task<PublisherClient> CreatePublisherAsync(CancellationToken cancellationToken)
        {
            var projectId = _options.ProjectId;
            var topicId = _options.TopicId;
            var subscriptionId = _options.SubscriptionId;

            // Subscription manage
            var subscription = await _pubSubResourceHelper.CreateSubscriptionAsync(
                projectId: projectId,
                topicId: topicId,
                subscriptionId: subscriptionId);

            // Publisher manage
            var publisher = await new PublisherClientBuilder
            {
                TopicName = subscription.TopicAsTopicName,
                EmulatorDetection = EmulatorDetection
            }.BuildAsync(cancellationToken);

            return publisher;
        }

        public async Task PublishMessagesAsync(CancellationToken cancellationToken = default)
        {
            var publisher = await CreatePublisherAsync(cancellationToken);

            try
            {
                var messageCount = 0;
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (Console.KeyAvailable)
                    {
                        var message = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(message))
                        {
                            continue;
                        }

                        var pubsubMessage = new PubsubMessage
                        {
                            Data = ByteString.CopyFromUtf8(message)
                        };

                        await publisher.PublishAsync(pubsubMessage);
                        _logger.LogInformation($"Published message: {message}");
                        _logger.LogInformation($"Published message count: {++messageCount}");

                        // Simulate some delay
                        await Task.Delay(300, cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Publishing cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Publishing failed");
            }
            finally
            {
                await publisher.ShutdownAsync(CancellationToken.None);
            }
        }
    }
}