namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub
{
    public interface IPubSubConsumer : IAsyncDisposable
    {
        Task<ISubscriptionHandle> StartAsync(
            PubSubTaskConfig config,
            Func<PubSubPayload, CancellationToken, Task> handleMessageAsync,
            CancellationToken cancellationToken = default);
    }
}