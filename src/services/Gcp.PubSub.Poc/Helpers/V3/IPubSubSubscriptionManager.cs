namespace Gcp.PubSub.Poc.Helpers.V3
{
    public interface IPubSubSubscriptionManager : IAsyncDisposable
    {
        int ActiveSubscriptionCount { get; }

        Task<ISubscriptionHandle> StartSubscriptionAsync(
            string subscriptionName, // 給訂閱一個名稱以便管理
            PubSubTaskConfig config,
            Func<PubSubPayload, CancellationToken, Task> handleMessageAsync,
            CancellationToken cancellationToken = default);

        Task StopSubscriptionAsync(string subscriptionName, CancellationToken cancellationToken = default);

        Task StopAllSubscriptionsAsync(CancellationToken cancellationToken = default);

        ISubscriptionHandle? GetSubscription(string subscriptionName);

        IEnumerable<ISubscriptionHandle> GetActiveSubscriptions();
    }
}