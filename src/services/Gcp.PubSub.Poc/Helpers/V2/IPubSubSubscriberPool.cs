using Google.Cloud.PubSub.V1;

namespace Gcp.PubSub.Poc.Helpers.V2
{
    public interface IPubSubSubscriberPool : IAsyncDisposable
    {
        Task<SubscriberClient> GetSubscriberAsync(string projectId, string subscriptionId);
    }
}