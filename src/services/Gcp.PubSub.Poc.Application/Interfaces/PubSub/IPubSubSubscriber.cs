namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub
{
    public interface IPubSubSubscriber : IAsyncDisposable
    {
        Task<IPubSubSubscriberHandle> StartAsync(
            PubSubTaskConfig config,
            Func<PubSubPayload, CancellationToken, Task> handleMessageAsync,
            CancellationToken cancellationToken = default);
    }
}