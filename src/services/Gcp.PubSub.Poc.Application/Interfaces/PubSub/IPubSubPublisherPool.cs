using Google.Cloud.PubSub.V1;

namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub
{
    public interface IPubSubPublisherPool : IAsyncDisposable
    {
        Task<PublisherClient> GetPublisherAsync(string projectId, string topicId);
    }
}