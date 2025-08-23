namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub
{
    public class PubSubOptions
    {
        public const string GcpPubSub = "Gcp:PubSub";

        public bool Emulated { get; set; }

        public string Endpoint => Environment.GetEnvironmentVariable("PUBSUB_EMULATOR_HOST")!;
        
        public string ProjectId => Environment.GetEnvironmentVariable("PUBSUB_PROJECT_ID")!;

        public string TopicId => Environment.GetEnvironmentVariable("PUBSUB_TOPIC_ID")!;

        public string SubscriptionId => Environment.GetEnvironmentVariable("PUBSUB_SUBSCRIPTION_ID")!;
    }
}