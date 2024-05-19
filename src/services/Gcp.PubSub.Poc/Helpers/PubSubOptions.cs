namespace Gcp.PubSub.Poc.Helpers
{
    public class PubSubOptions
    {
        public const string GcpPubSub = "Gcp:PubSub";

        public bool Emulated { get; set; }

        public string ProjectId => Environment.GetEnvironmentVariable("PUBSUB_PROJECT_ID");

        public string TopicId { get; set; } = "something-go-wrong";

        public string SubscriptionId { get; set; } = "regist-something";
    }
}