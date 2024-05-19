namespace Gcp.PubSub.Poc.Producer.Services
{
    public interface IProducerService
    {
        Task PublishMessagesAsync(CancellationToken cancellationToken = default);
    }
}