namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub
{
    public class PubSubTaskConfig
    {
        public string ProjectId { get; set; }
        
        public string TopicId { get; set; }
        
        public string SubscriptionId { get; set; }
    }
}