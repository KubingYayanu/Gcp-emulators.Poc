namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub.Subscriber
{
    /// <summary>
    /// Subscriber Registration 跟踪器
    /// </summary>
    public class SubscriberRegistration
    {
        public string SubscriberId { get; set; }
        
        public Func<PubSubPayload, CancellationToken, Task> MessageHandler { get; set; }
        
        public DateTimeOffset RegisteredAt { get; set; }
    }
}