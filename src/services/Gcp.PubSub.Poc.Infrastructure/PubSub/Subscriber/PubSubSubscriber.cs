using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub.Subscriber;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;

namespace Gcp.PubSub.Poc.Infrastructure.PubSub.Subscriber
{
    public class PubSubSubscriber : IPubSubSubscriber
    {
        private readonly IPubSubSubscriberPool _subscriberPool;
        private readonly ILogger<PubSubSubscriber> _logger;
        private readonly string _subscriberId;

        public PubSubSubscriber(
            IPubSubSubscriberPool subscriberPool,
            ILogger<PubSubSubscriber> logger)
        {
            _subscriberPool = subscriberPool;
            _logger = logger;
            _subscriberId = Guid.NewGuid().ToString("N")[..8];
        }

        public async Task<IPubSubSubscriberHandle> StartAsync(
            PubSubTaskConfig config,
            Func<PubSubSubscriberPayload, CancellationToken, Task> messageHandler,
            CancellationToken cancellationToken = default)
        {
            var projectId = config.ProjectId;
            var subscriptionId = config.SubscriptionId;
            var wrappedHandler = CreateWrappedHandler(messageHandler, config);

            var subscriber = await _subscriberPool.GetOrCreateSubscriberAsync(
                subscriberId: _subscriberId,
                projectId: projectId,
                subscriptionId: subscriptionId,
                messageHandler: wrappedHandler,
                ackDeadlineSeconds: config.SubscriberAckDeadline,
                cancellationToken: cancellationToken);

            // 不要 await StartAsync, 讓它在背景執行
            var startTask = subscriber.StartAsync(async (message, handlerCancellationToken) =>
            {
                try
                {
                    var payload = new PubSubSubscriberPayload
                    {
                        MessageId = message.MessageId,
                        Message = message.Data.ToStringUtf8(),
                        Attributes = new Dictionary<string, string>(message.Attributes)
                    };

                    await wrappedHandler(payload, handlerCancellationToken);
                    return SubscriberClient.Reply.Ack;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        exception: ex,
                        message: "Message processing failed for SubscriberId: {SubscriberId}, "
                                 + "ProjectId: {ProjectId}, SubscriptionId: {SubscriptionId}",
                        args: [_subscriberId, projectId, subscriptionId]);
                    return SubscriberClient.Reply.Nack;
                }
            });

            _logger.LogInformation(
                message: "Subscriber {SubscriberId} started for subscription. "
                         + "ProjectId: {ProjectId}, SubscriptionId: {SubscriptionId}",
                args:
                [
                    _subscriberId,
                    projectId,
                    subscriptionId
                ]);

            // 立即返回 handle，StartAsync 在背景執行
            return new PubSubSubscriberHandle(
                subscriberId: _subscriberId,
                config: config,
                subscriberPool: _subscriberPool,
                subscriber: subscriber,
                startTask: startTask,
                logger: _logger);
        }

        private Func<PubSubSubscriberPayload, CancellationToken, Task> CreateWrappedHandler(
            Func<PubSubSubscriberPayload, CancellationToken, Task> originalHandler,
            PubSubTaskConfig config)
        {
            return async (payload, cancellationToken) =>
            {
                try
                {
                    await originalHandler(payload, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        exception: ex,
                        message: "Handler failed for consumer {ConsumerId}, subscription {ProjectId}:{SubscriptionId}",
                        args:
                        [
                            _subscriberId,
                            config.ProjectId,
                            config.SubscriptionId
                        ]);
                    throw;
                }
            };
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}