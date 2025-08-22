namespace Gcp.PubSub.Poc.Helpers.V3
{
    public interface ISubscriptionHandle : IAsyncDisposable
    {
        string ConsumerId { get; }

        string ProjectId { get; }

        string SubscriptionId { get; }

        bool IsRunning { get; }

        Task StartTask { get; }

        Task StopAsync(CancellationToken cancellationToken = default);

        Task WaitForCompletionAsync(CancellationToken cancellationToken = default);
    }
}