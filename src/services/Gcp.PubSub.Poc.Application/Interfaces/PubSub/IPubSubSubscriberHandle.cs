namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub
{
    public interface IPubSubSubscriberHandle : IAsyncDisposable
    {
        string SubscriberId { get; }

        string ProjectId { get; }

        string SubscriptionId { get; }

        bool IsRunning { get; }

        Task StartTask { get; }

        Task StopAsync(CancellationToken cancellationToken = default);

        Task WaitForCompletionAsync(CancellationToken cancellationToken = default);
    }
}