using Gcp.PubSub.Poc.Helpers;
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
        }
    }
}