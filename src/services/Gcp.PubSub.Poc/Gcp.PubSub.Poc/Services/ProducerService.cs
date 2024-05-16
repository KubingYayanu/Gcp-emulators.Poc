using Google.Api.Gax;
using Google.Cloud.PubSub.V1;

namespace Redis.Poc.Services
{
    public class ProducerService : IProducerService
    {
        private const string ProjectId = "lets-have-some-fun";
        private const string TopicId = "something-go-wrong";
        private const string SubscriptionId = "regist-something";

        public async Task Run()
        {
            // Topic manage
            var publisherService = await new PublisherServiceApiClientBuilder
            {
                EmulatorDetection = EmulatorDetection.EmulatorOnly
            }.BuildAsync();

            var topicName = new TopicName(ProjectId, TopicId);
            var topic = await publisherService.CreateTopicAsync(topicName);

            // Subscription manage
            var subscriberService = await new SubscriberServiceApiClientBuilder
            {
                EmulatorDetection = EmulatorDetection.EmulatorOnly
            }.BuildAsync();

            var subscriptionName = new SubscriptionName(ProjectId, SubscriptionId);
            var subscription = await subscriberService.CreateSubscriptionAsync(
                subscriptionName,
                topicName,
                pushConfig: null,
                ackDeadlineSeconds: 60);

            // Publisher manage
            var publisher = await new PublisherClientBuilder
            {
                TopicName = topicName,
                EmulatorDetection = EmulatorDetection.EmulatorOnly
            }.BuildAsync();

            await publisher.PublishAsync("Hello, Pubsub");
            await publisher.ShutdownAsync(TimeSpan.FromSeconds(15));

            // Subscriber manage
            var subscriber = await new SubscriberClientBuilder
            {
                SubscriptionName = subscriptionName,
                EmulatorDetection = EmulatorDetection.EmulatorOnly
            }.BuildAsync();
            var receivedMessages = new List<PubsubMessage>();

            await subscriber.StartAsync((msg, cancellationToken) =>
            {
                receivedMessages.Add(msg);
                Console.WriteLine($"Received message {msg.MessageId} published at {msg.PublishTime.ToDateTime()}");
                Console.WriteLine($"Text: '{msg.Data.ToStringUtf8()}'");

                subscriber.StopAsync(TimeSpan.FromSeconds(15));

                return Task.FromResult(SubscriberClient.Reply.Ack);
            });

            throw new NotImplementedException();
        }
    }
}