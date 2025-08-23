namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub
{
    public interface IPubSubPublisher
    {
        Task<string> PublishAsync(
            PubSubTaskConfig config,
            PubSubPayload payload,
            CancellationToken cancellationToken = default);
    }
}