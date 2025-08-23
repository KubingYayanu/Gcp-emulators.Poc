namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub
{
    /// <summary>
    /// Producer Registration 跟踪器
    /// </summary>
    public class ProducerRegistration
    {
        public string ProducerId { get; set; }

        public DateTimeOffset RegisteredAt { get; set; }
    }
}