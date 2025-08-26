namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub.Publisher
{
    public interface IPubSubPublisherHandle : IAsyncDisposable
    {
        string PublisherId { get; }

        string ProjectId { get; }

        string TopicId { get; }

        Task<string> PublishAsync(
            PubSubPublisherPayload payload,
            CancellationToken cancellationToken = default);

        Task ShutdownAsync(CancellationToken cancellationToken = default);
    }
}