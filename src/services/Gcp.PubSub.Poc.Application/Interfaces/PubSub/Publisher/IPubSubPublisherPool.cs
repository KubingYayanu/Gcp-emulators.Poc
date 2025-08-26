using Google.Cloud.PubSub.V1;

namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub.Publisher
{
    public interface IPubSubPublisherPool : IAsyncDisposable
    {
        Task<PublisherClient> GetOrCreatePublisherAsync(
            string publisherId,
            string projectId,
            string topicId,
            CancellationToken cancellationToken = default);
        
        Task RemovePublisherAsync(
            string publisherId, 
            string projectId,
            string topicId,
            CancellationToken cancellationToken = default);
    }
}