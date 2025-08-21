using Google.Cloud.PubSub.V1;

namespace Gcp.PubSub.Poc.Helpers.V2
{
    public interface IPubSubPublisherPool : IAsyncDisposable
    {
        Task<PublisherClient> GetPublisherAsync(string projectId, string topicId);
    }
}