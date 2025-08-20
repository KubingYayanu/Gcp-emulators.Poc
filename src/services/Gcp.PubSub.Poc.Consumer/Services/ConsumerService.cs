using Gcp.PubSub.Poc.Helpers;
using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gcp.PubSub.Poc.Consumer.Services
{
    public class ConsumerService : IConsumerService
    {
        private readonly IPubSubConsumer _pubSubConsumer;
        private readonly PubSubOptions _options;
        private readonly ILogger<ConsumerService> _logger;

        public ConsumerService(
            IPubSubConsumer pubSubConsumer,
            IOptions<PubSubOptions> options,
            ILogger<ConsumerService> logger)
        {
            _pubSubConsumer = pubSubConsumer;
            _options = options.Value;
            _logger = logger;
        }

        private EmulatorDetection EmulatorDetection => _options.Emulated
            ? EmulatorDetection.EmulatorOnly
            : EmulatorDetection.ProductionOnly;

        // private async Task<SubscriberClient> CreateSubscriberAsync(CancellationToken cancellationToken)
        // {
        //     var projectId = _options.ProjectId;
        //     var topicId = _options.TopicId;
        //     var subscriptionId = _options.SubscriptionId;
        //
        //     // Subscription manage
        //     var subscription = await _pubSubResourceHelper.CreateSubscriptionAsync(
        //         projectId: projectId,
        //         topicId: topicId,
        //         subscriptionId: subscriptionId);
        //     var subscriptionName = subscription.SubscriptionName;
        //
        //     // Subscriber manage
        //     var subscriber = await new SubscriberClientBuilder
        //     {
        //         SubscriptionName = subscriptionName,
        //         EmulatorDetection = EmulatorDetection
        //     }.BuildAsync(cancellationToken);
        //
        //     return subscriber;
        // }

        public async Task PullMessagesAsync(CancellationToken cancellationToken = default)
        {
            var config = new PubSubTaskConfig
            {
                ProjectId = _options.ProjectId,
                TopicId = _options.TopicId,
                SubscriptionId = _options.SubscriptionId,
            };

            await _pubSubConsumer.StartAsync(
                config: config,
                handleMessageAsync: (message, cancellationToken) =>
                {
                    _logger.LogInformation("Received message {MessageMessage}", message.Message);
                    return Task.CompletedTask;
                },
                cancellationToken: cancellationToken);


            // var subscriber = await CreateSubscriberAsync(cancellationToken);
            // var receivedMessages = new List<PubsubMessage>();
            //
            // cancellationToken.Register(async () =>
            // {
            //     _logger.LogInformation("Cancellation requested, stopping subscriber...");
            //     await subscriber.StopAsync(CancellationToken.None); // Provide a token that doesn't cancel
            // });
            //
            // await subscriber.StartAsync((msg, cancellationToken) =>
            // {
            //     receivedMessages.Add(msg);
            //     _logger.LogInformation($"Received message {msg.MessageId} published at {msg.PublishTime.ToDateTime()}");
            //     _logger.LogInformation($"Text: '{msg.Data.ToStringUtf8()}'");
            //
            //     return Task.FromResult(SubscriberClient.Reply.Ack);
            // });
        }
    }
}