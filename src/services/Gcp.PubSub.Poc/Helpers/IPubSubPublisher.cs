namespace Gcp.PubSub.Poc.Helpers
{
    public interface IPubSubPublisher
    {
        Task<string> PublishAsync(
            PubSubTaskConfig config,
            PubSubPayload payload,
            CancellationToken cancellationToken = default);
    }
}