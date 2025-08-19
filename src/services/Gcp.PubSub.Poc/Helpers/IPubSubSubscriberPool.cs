using Google.Cloud.PubSub.V1;

namespace Gcp.PubSub.Poc.Helpers
{
    public interface IPubSubSubscriberPool : IAsyncDisposable
    {
        Task<SubscriberClient> GetSubscriberAsync(string projectId, string subscriptionId);
    }
}