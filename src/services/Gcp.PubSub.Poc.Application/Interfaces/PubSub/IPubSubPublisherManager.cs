namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub
{
    public interface IPubSubPublisherManager : IAsyncDisposable
    {
        int ActivePublisherCount { get; }

        Task<IPubSubPublisherHandle> StartPublisherAsync(
            string publisherName, // 給發布一個名稱以便管理
            PubSubTaskConfig config,
            CancellationToken cancellationToken = default);

        Task StopPublisherAsync(
            string publisherName,
            CancellationToken cancellationToken = default);

        Task StopAllPublishersAsync(CancellationToken cancellationToken = default);

        IPubSubPublisherHandle? GetPublisherHandle(string publisherName);

        IEnumerable<IPubSubPublisherHandle> GetActivePublisherHandles();
    }
}