namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub
{
    public interface IPubSubPublisher
    {
        Task<IPublisherHandle> StartAsync(
            PubSubTaskConfig config,
            CancellationToken cancellationToken = default);
    }
}