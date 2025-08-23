using Google.Cloud.PubSub.V1;

namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub
{
    public interface IPubSubSubscriberPool : IAsyncDisposable
    {
        Task<SubscriberClient> GetOrCreateSubscriberAsync(
            string consumerId,
            string projectId, 
            string subscriptionId,
            Func<PubSubPayload, CancellationToken, Task> handler);
    
        Task RemoveSubscriberAsync(string consumerId, string projectId, string subscriptionId);
    }
}