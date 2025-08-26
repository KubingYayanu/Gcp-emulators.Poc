using Gcp.PubSub.Poc.Domain.Queues.Options;
using Gcp.PubSub.Poc.Helpers;
using Gcp.PubSub.Poc.Helpers.V3;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gcp.PubSub.Poc.Consumer.Services
{
    public class ConsumerService : IConsumerService
    {
        private readonly IPubSubSubscriptionManager _subscriptionManager;
        private readonly WorkerQueueOptions _options;
        private readonly ILogger<ConsumerService> _logger;

        public ConsumerService(
            IPubSubSubscriptionManager subscriptionManager,
            IOptions<WorkerQueueOptions> options,
            ILogger<ConsumerService> logger)
        {
            _subscriptionManager = subscriptionManager;
            _options = options.Value;
            _logger = logger;
        }

        public async Task PullMessagesAsync(CancellationToken cancellationToken = default)
        {
            var config = new PubSubTaskConfig
            {
                ProjectId = _options.PublisherA.ProjectId,
                TopicId = _options.PublisherA.TopicId,
                SubscriptionId = _options.PublisherA.SubscriptionId,
            };

            var subscriptionName = nameof(ConsumerService);
            await _subscriptionManager.StartSubscriptionAsync(
                subscriptionName: subscriptionName,
                config: config,
                handleMessageAsync: (payload, ct) => ProcessMessage(subscriptionName, payload, ct),
                cancellationToken: cancellationToken);
        }

        private async Task ProcessMessage(
            string subscriptionName,
            PubSubPayload payload,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Subscription}] Processing: {Message}", subscriptionName, payload.Message);
            await Task.Delay(100, cancellationToken);
        }
    }
}