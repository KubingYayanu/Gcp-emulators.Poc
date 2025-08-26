namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub.Publisher
{
    /// <summary>
    /// Publisher Registration 跟踪器
    /// </summary>
    public class PublisherRegistration
    {
        public string PublisherId { get; set; }

        public DateTimeOffset RegisteredAt { get; set; }
    }
}