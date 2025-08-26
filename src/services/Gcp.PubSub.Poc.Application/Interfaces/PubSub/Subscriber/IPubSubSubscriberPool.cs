using Google.Cloud.PubSub.V1;

namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub.Subscriber
{
    public interface IPubSubSubscriberPool : IAsyncDisposable
    {
        Task<SubscriberClient> GetOrCreateSubscriberAsync(
            string subscriberId,
            string projectId, 
            string subscriptionId,
            Func<PubSubPayload, CancellationToken, Task> messageHandler,
            CancellationToken cancellationToken = default);
    
        Task RemoveSubscriberAsync(
            string subscriberId,
            string projectId,
            string subscriptionId,
            CancellationToken cancellationToken = default);
    }
}