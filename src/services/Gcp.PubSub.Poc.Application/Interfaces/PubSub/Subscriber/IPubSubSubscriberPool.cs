using Google.Cloud.PubSub.V1;

namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub.Subscriber
{
    public interface IPubSubSubscriberPool : IAsyncDisposable
    {
        Task<SubscriberClient> GetOrCreateSubscriberAsync(
            string subscriberId,
            string projectId,
            string subscriptionId,
            long? ackDeadlineSeconds = null,
            CancellationToken cancellationToken = default);
    
        Task RemoveSubscriberAsync(
            string subscriberId,
            string projectId,
            string subscriptionId,
            CancellationToken cancellationToken = default);
    }
}