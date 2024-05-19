namespace Gcp.PubSub.Poc.Helpers
{
    public class PubSubOptions
    {
        public const string GcpPubSub = "Gcp:PubSub";

        public bool Emulated { get; set; }

        public string? ConnectionString { get; set; }

        public string ProjectId { get; set; } = "lets-have-some-fun";

        public string TopicId { get; set; } = "something-go-wrong";

        public string SubscriptionId { get; set; } = "regist-something";
    }
}