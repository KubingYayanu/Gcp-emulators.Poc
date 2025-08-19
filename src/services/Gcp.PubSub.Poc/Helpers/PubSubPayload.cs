namespace Gcp.PubSub.Poc.Helpers
{
    public class PubSubPayload
    {
        public string Message { get; set; }
        
        public Dictionary<string, string> Attributes { get; set; } = new();
    }
}