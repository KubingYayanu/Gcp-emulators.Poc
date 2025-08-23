using Google.Cloud.PubSub.V1;

namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub
{
    public interface IPubSubPublisherPool : IAsyncDisposable
    {
        Task<PublisherClient> GetOrCreatePublisherAsync(
            string producerId,
            string projectId,
            string topicId);
        
        Task RemovePublisherAsync(
            string producerId, 
            string projectId,
            string topicId);
    }
}