namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub
{
    public interface IPubSubPublisherHandle : IAsyncDisposable
    {
        string PublisherId { get; }

        string ProjectId { get; }

        string TopicId { get; }

        Task<string> PublishAsync(
            PubSubPayload payload,
            CancellationToken cancellationToken = default);

        Task ShutdownAsync(CancellationToken cancellationToken = default);
    }
}