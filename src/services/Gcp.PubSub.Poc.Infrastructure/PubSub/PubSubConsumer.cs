using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;

namespace Gcp.PubSub.Poc.Infrastructure.PubSub
{
    public class PubSubConsumer : IPubSubConsumer
    {
        private readonly IPubSubSubscriberPool _subscriberPool;
        private readonly ILogger<PubSubConsumer> _logger;
        private readonly string _consumerId;

        public PubSubConsumer(
            IPubSubSubscriberPool subscriberPool,
            ILogger<PubSubConsumer> logger)
        {
            _subscriberPool = subscriberPool;
            _logger = logger;
            _consumerId = Guid.NewGuid().ToString("N")[..8];
        }

        public async Task<ISubscriptionHandle> StartAsync(
            PubSubTaskConfig config,
            Func<PubSubPayload, CancellationToken, Task> handleMessageAsync,
            CancellationToken cancellationToken = default)
        {
            var wrappedHandler = CreateWrappedHandler(handleMessageAsync, config);

            var subscriber = await _subscriberPool.GetOrCreateSubscriberAsync(
                _consumerId,
                config.ProjectId,
                config.SubscriptionId,
                wrappedHandler);

            // 關鍵修正：不要 await StartAsync，讓它在背景執行
            var startTask = subscriber.StartAsync(async (message, handlerCancellationToken) =>
            {
                try
                {
                    var pubSubMessage = new PubSubPayload
                    {
                        Message = message.Data.ToStringUtf8(),
                        Attributes = new Dictionary<string, string>(message.Attributes)
                    };

                    await wrappedHandler(pubSubMessage, handlerCancellationToken);
                    return SubscriberClient.Reply.Ack;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Message processing failed for consumer {ConsumerId}", _consumerId);
                    return SubscriberClient.Reply.Nack;
                }
            });

            _logger.LogInformation("Consumer {ConsumerId} started for subscription {ProjectId}:{SubscriptionId}",
                _consumerId,
                config.ProjectId,
                config.SubscriptionId);
            
            // await startTask;

            // 立即返回 handle，StartAsync 在背景執行
            return new SubscriptionHandle(_subscriberPool, subscriber, startTask, _consumerId, config, _logger);
        }

        private Func<PubSubPayload, CancellationToken, Task> CreateWrappedHandler(
            Func<PubSubPayload, CancellationToken, Task> originalHandler,
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
                    _logger.LogError(ex,
                        "Handler failed for consumer {ConsumerId}, subscription {ProjectId}:{SubscriptionId}",
                        _consumerId,
                        config.ProjectId,
                        config.SubscriptionId);
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