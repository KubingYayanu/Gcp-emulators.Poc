using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;

namespace Gcp.PubSub.Poc.Helpers
{
    public class PubSubConsumer : IPubSubConsumer
    {
        private readonly IPubSubSubscriberPool _subscriberPool;
        private readonly ILogger<PubSubConsumer> _logger;

        public PubSubConsumer(
            IPubSubSubscriberPool subscriberPool,
            ILogger<PubSubConsumer> logger)
        {
            _subscriberPool = subscriberPool;
            _logger = logger;
        }

        public async Task StartAsync(
            PubSubTaskConfig config,
            Func<PubSubPayload, CancellationToken, Task> handleMessageAsync,
            CancellationToken cancellationToken = default)
        {
            var subscriber = await _subscriberPool.GetSubscriberAsync(
                projectId: config.ProjectId,
                subscriptionId: config.SubscriptionId);

            await subscriber.StartAsync(async (message, handlerCancellationToken) =>
            {
                try
                {
                    var pubSubMessage = new PubSubPayload
                    {
                        Message = message.Data.ToStringUtf8(),
                        Attributes = new Dictionary<string, string>(message.Attributes)
                    };

                    await handleMessageAsync(pubSubMessage, handlerCancellationToken);

                    return SubscriberClient.Reply.Ack;
                }
                catch (Exception ex)
                {
                    // TODO: Send to Dead Letter Queue (DLQ)
                    _logger.LogError(ex, "Message processing failed");
                    return SubscriberClient.Reply.Nack;
                }
            });
        }

        public async Task StopAsync(
            PubSubTaskConfig config,
            CancellationToken cancellationToken = default)
        {
            var subscriber = await _subscriberPool.GetSubscriberAsync(
                projectId: config.ProjectId,
                subscriptionId: config.SubscriptionId);

            await subscriber.StopAsync(TimeSpan.FromSeconds(30));
        }
    }
}