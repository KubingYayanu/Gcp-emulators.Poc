using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using Grpc.Core;
using Microsoft.Extensions.Options;

namespace Gcp.PubSub.Poc.Helpers
{
    public class PubSubResourceHelper : IPubSubResourceHelper
    {
        private readonly Lazy<PublisherServiceApiClient> _publisherService;
        private readonly Lazy<SubscriberServiceApiClient> _subscripberService;
        private readonly PubSubOptions _options;

        public PubSubResourceHelper(
            IOptions<PubSubOptions> options)
        {
            _options = options.Value;
            _publisherService = new Lazy<PublisherServiceApiClient>(() =>
            {
                var publisherService = new PublisherServiceApiClientBuilder
                {
                    EmulatorDetection = EmulatorDetection
                }.Build();

                return publisherService;
            });
            _subscripberService = new Lazy<SubscriberServiceApiClient>(() =>
            {
                var subscriberService = new SubscriberServiceApiClientBuilder
                {
                    EmulatorDetection = EmulatorDetection
                }.Build();

                return subscriberService;
            });
        }

        private EmulatorDetection EmulatorDetection => _options.Emulated
            ? EmulatorDetection.EmulatorOnly
            : EmulatorDetection.ProductionOnly;

        private PublisherServiceApiClient PublisherService => _publisherService.Value;

        private SubscriberServiceApiClient SubscriberService => _subscripberService.Value;

        public async Task<Topic> CreateTopicAsync(string projectId, string topicId)
        {
            var topicName = new TopicName(projectId, topicId);
            try
            {
                return await PublisherService.GetTopicAsync(topicName);
            }
            catch (RpcException e) when (e.Status.StatusCode == StatusCode.NotFound)
            {
                return await PublisherService.CreateTopicAsync(topicName);
            }
        }

        public async Task<Subscription> CreateSubscriptionAsync(
            string projectId,
            string topicId,
            string subscriptionId)
        {
            var subscriptionName = new SubscriptionName(projectId, subscriptionId);
            var topic = await CreateTopicAsync(projectId, topicId);
            var topicName = topic.TopicName;

            try
            {
                return await SubscriberService.GetSubscriptionAsync(subscriptionName);
            }
            catch (RpcException e) when (e.Status.StatusCode == StatusCode.NotFound)
            {
                return await SubscriberService.CreateSubscriptionAsync(
                    subscriptionName,
                    topicName,
                    pushConfig: null,
                    ackDeadlineSeconds: 60);
            }
        }
    }
}