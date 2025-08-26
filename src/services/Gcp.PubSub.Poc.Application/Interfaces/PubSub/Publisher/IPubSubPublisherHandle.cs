using Google.Cloud.PubSub.V1;

namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub.Publisher
{
    public interface IPubSubPublisherHandle : IAsyncDisposable
    {
        string PublisherId { get; }

        string ProjectId { get; }

        string TopicId { get; }

        Task<string> PublishAsync(PubsubMessage payload);

        Task ShutdownAsync(CancellationToken cancellationToken = default);
    }
}