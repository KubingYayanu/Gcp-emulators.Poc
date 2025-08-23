namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub
{
    public class PubSubPayload
    {
        public string Message { get; set; }
        
        public Dictionary<string, string> Attributes { get; set; } = new();
    }
}