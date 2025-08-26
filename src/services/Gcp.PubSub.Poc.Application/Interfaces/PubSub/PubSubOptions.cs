namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub
{
    public class PubSubOptions
    {
        public const string GcpPubSub = "Gcp:PubSub";

        public bool Emulated { get; set; }

        public string Host { get; set; }
    }
}