namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub.Publisher
{
    public interface IPubSubPublisher
    {
        Task<IPubSubPublisherHandle> StartAsync(
            PubSubTaskConfig config,
            CancellationToken cancellationToken = default);
    }
}