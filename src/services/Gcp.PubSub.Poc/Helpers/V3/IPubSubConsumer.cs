namespace Gcp.PubSub.Poc.Helpers.V3
{
    public interface IPubSubConsumer : IAsyncDisposable
    {
        Task<ISubscriptionHandle> StartAsync(
            PubSubTaskConfig config,
            Func<PubSubPayload, CancellationToken, Task> handleMessageAsync,
            CancellationToken cancellationToken = default);
    }
}