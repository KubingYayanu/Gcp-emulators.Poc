namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub.Subscriber
{
    public interface IPubSubSubscriberManager : IAsyncDisposable
    {
        int ActiveSubscriberCount { get; }

        /// <summary>
        /// 啟動一個新的訂閱者, 並由管理器負責其生命週期
        /// </summary>
        /// <param name="subscriberName">給訂閱一個名稱以便管理</param>
        /// <param name="config"></param>
        /// <param name="messageHandler"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IPubSubSubscriberHandle> StartSubscriberAsync<T>(
            string subscriberName,
            PubSubTaskConfig config,
            Func<PubSubEnvelope<T>, CancellationToken, Task> messageHandler,
            CancellationToken cancellationToken = default);

        Task StopSubscriberAsync(
            string subscriberName,
            CancellationToken cancellationToken = default);

        Task StopAllSubscribersAsync(CancellationToken cancellationToken = default);

        IPubSubSubscriberHandle? GetSubscriberHandle(string subscriberName);

        IEnumerable<IPubSubSubscriberHandle> GetActiveSubscriberHandles();
    }
}