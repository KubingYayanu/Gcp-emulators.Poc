using Gcp.PubSub.Poc.Helpers;
using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Options;

namespace Gcp.PubSub.Poc.Producer.Services
{
    public class ProducerService : IProducerService
    {
        private readonly IPubSubResourceHelper _pubSubResourceHelper;
        private readonly PubSubOptions _options;

        public ProducerService(
            IPubSubResourceHelper pubSubResourceHelper,
            IOptions<PubSubOptions> options)
        {
            _pubSubResourceHelper = pubSubResourceHelper;
            _options = options.Value;
        }

        public async Task PublishMessagesAsync()
        {
            var projectId = _options.ProjectId;
            var topicId = _options.TopicId;
            var subscriptionId = _options.SubscriptionId;

            // Topic manage
            var topic = await _pubSubResourceHelper.CreateTopicAsync(projectId, topicId);
            var topicName = topic.TopicName;

            // Subscription manage
            var subscription = await _pubSubResourceHelper.CreateSubscriptionAsync(
                projectId: projectId,
                topicId: topicId,
                subscriptionId: subscriptionId);
            var subscriptionName = subscription.SubscriptionName;
            
            // Publisher manage
            var publisher = await new PublisherClientBuilder
            {
                TopicName = topicName,
                EmulatorDetection = EmulatorDetection.EmulatorOnly
            }.BuildAsync();

            await publisher.PublishAsync("Hello, Pubsub");
            await publisher.ShutdownAsync(TimeSpan.FromSeconds(15));
        }
    }
}