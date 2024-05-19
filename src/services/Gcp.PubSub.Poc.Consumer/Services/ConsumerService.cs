using Gcp.PubSub.Poc.Helpers;
using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gcp.PubSub.Poc.Consumer.Services
{
    public class ConsumerService : IConsumerService
    {
        private readonly IPubSubResourceHelper _pubSubResourceHelper;
        private readonly PubSubOptions _options;
        private readonly ILogger<ConsumerService> _logger;

        public ConsumerService(
            IPubSubResourceHelper pubSubResourceHelper,
            IOptions<PubSubOptions> options,
            ILogger<ConsumerService> logger)
        {
            _pubSubResourceHelper = pubSubResourceHelper;
            _options = options.Value;
            _logger = logger;
        }

        private EmulatorDetection EmulatorDetection => _options.Emulated
            ? EmulatorDetection.EmulatorOnly
            : EmulatorDetection.ProductionOnly;

        public async Task PullMessagesAsync()
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

            // Subscriber manage
            var subscriber = await new SubscriberClientBuilder
            {
                SubscriptionName = subscriptionName,
                EmulatorDetection = EmulatorDetection
            }.BuildAsync();
            var receivedMessages = new List<PubsubMessage>();

            await subscriber.StartAsync((msg, cancellationToken) =>
            {
                receivedMessages.Add(msg);
                _logger.LogInformation($"Received message {msg.MessageId} published at {msg.PublishTime.ToDateTime()}");
                _logger.LogInformation($"Text: '{msg.Data.ToStringUtf8()}'");

                subscriber.StopAsync(TimeSpan.FromSeconds(15));

                return Task.FromResult(SubscriberClient.Reply.Ack);
            });
        }
    }
}