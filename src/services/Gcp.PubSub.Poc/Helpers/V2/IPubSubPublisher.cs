namespace Gcp.PubSub.Poc.Helpers.V2
{
    public interface IPubSubPublisher
    {
        Task<string> PublishAsync(
            PubSubTaskConfig config,
            PubSubPayload payload,
            CancellationToken cancellationToken = default);
    }
}