using Google.Cloud.PubSub.V1;

namespace Gcp.PubSub.Poc.Helpers.V1
{
    public interface IPubSubResourceHelper
    {
        Task<Topic> CreateTopicAsync(string projectId, string topicId);

        Task<Subscription> CreateSubscriptionAsync(
            string projectId,
            string topicId,
            string subscriptionId);
    }
}