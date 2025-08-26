namespace Gcp.PubSub.Poc.Domain.Queues.Options
{
    public class QueueSection
    {
        public string ProjectId { get; set; }

        public string TopicId { get; set; }

        public string SubscriptionId { get; set; }

        public long? SubscriberAckDeadline { get; set; }
    }
}