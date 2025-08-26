namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub
{
    public class PubSubTaskConfig
    {
        public PubSubTaskConfig(
            string projectId,
            string topicId,
            string subscriptionId,
            long? subscriberAckDeadline = null)
        {
            ProjectId = projectId;
            TopicId = topicId;
            SubscriptionId = subscriptionId;
            SubscriberAckDeadline = subscriberAckDeadline;
        }

        public string ProjectId { get; set; }

        public string TopicId { get; set; }

        public string SubscriptionId { get; set; }

        public long? SubscriberAckDeadline { get; set; }
    }
}