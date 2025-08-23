namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub
{
    /// <summary>
    /// Consumer Registration 跟踪器
    /// </summary>
    public class ConsumerRegistration
    {
        public string ConsumerId { get; set; }
        
        public Func<PubSubPayload, CancellationToken, Task> Handler { get; set; }
        
        public DateTimeOffset RegisteredAt { get; set; }
    }
}