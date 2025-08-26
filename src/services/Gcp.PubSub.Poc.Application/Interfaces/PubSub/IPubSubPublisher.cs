namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub
{
    public interface IPubSubPublisher
    {
        Task<IPubSubPublisherHandle> StartAsync(
            PubSubTaskConfig config,
            CancellationToken cancellationToken = default);
    }
}