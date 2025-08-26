namespace Gcp.PubSub.Poc.Helpers
{
    public class PubSubOptions
    {
        public const string GcpPubSub = "Gcp:PubSub";

        public bool Emulated { get; set; }

        public string Host { get; set; }
    }
}