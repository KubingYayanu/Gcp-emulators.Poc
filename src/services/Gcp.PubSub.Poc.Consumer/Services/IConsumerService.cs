namespace Gcp.PubSub.Poc.Consumer.Services
{
    public interface IConsumerService
    {
        Task PullMessagesAsync();
    }
}