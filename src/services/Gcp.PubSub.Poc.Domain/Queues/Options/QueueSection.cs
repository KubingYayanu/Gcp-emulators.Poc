namespace Gcp.PubSub.Poc.Domain.Queues.Options
{
    public class QueueSection
    {
        public string ProjectId { get; set; }

        public string TopicId { get; set; }

        public string SubscriptionId { get; set; }

        /// <summary>
        /// Min: 10 seconds, Max: 600 seconds
        /// </summary>
        public long? AckDeadline { get; set; }
    }
}