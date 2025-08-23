using Gcp.PubSub.Poc.Application.Interfaces.Jobs;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Gcp.PubSub.Poc.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gcp.PubSub.Poc.Application.Services
{
    public class PublisherA : IJobService
    {
        private readonly IPubSubPublisher _pubSubPublisher;
        private readonly PubSubOptions _options;
        private readonly ILogger<PublisherA> _logger;

        public PublisherA(
            IPubSubPublisher pubSubPublisher,
            IOptions<PubSubOptions> options,
            ILogger<PublisherA> logger)
        {
            _pubSubPublisher = pubSubPublisher;
            _options = options.Value;
            _logger = logger;
        }

        public JobType JobType => JobType.PublisherA;

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                var config = new PubSubTaskConfig
                {
                    ProjectId = _options.ProjectId,
                    TopicId = _options.TopicId,
                    SubscriptionId = _options.SubscriptionId,
                };

                while (!cancellationToken.IsCancellationRequested)
                {
                    if (Console.KeyAvailable)
                    {
                        var message = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(message))
                        {
                            continue;
                        }

                        await _pubSubPublisher.PublishAsync(
                            config: config,
                            payload: new PubSubPayload
                            {
                                Message = message,
                                Attributes = new Dictionary<string, string>
                                {
                                    { "timestamp", DateTime.UtcNow.ToString("o") },
                                    { "source", "console" }
                                }
                            },
                            cancellationToken: cancellationToken);

                        _logger.LogInformation("Published message: {Message}", message);

                        await Task.Delay(300, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while running PublisherA job");
            }
        }
    }
}