namespace Gcp.PubSub.Poc.Helpers
{
    public interface IPubSubConsumer
    {
        Task StartAsync(
            PubSubTaskConfig config,
            Func<PubSubPayload, CancellationToken, Task> handleMessageAsync,
            CancellationToken cancellationToken = default);
        
        Task StopAsync(
            PubSubTaskConfig config,
            CancellationToken cancellationToken = default);
    }
}