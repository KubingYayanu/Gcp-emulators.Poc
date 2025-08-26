namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub.Subscriber
{
    public interface IPubSubSubscriber : IAsyncDisposable
    {
        /// <summary>
        /// 啟動背景訂閱工作
        /// </summary>
        /// <param name="config"></param>
        /// <param name="handleMessageAsync">
        /// 1. 若工作太長，建議把消息丟到背景 job/queue 處理，再 ack 掉 Pub/Sub 訊息
        /// 2. 冪等處理
        /// 3. 調整 AckDeadline
        /// </param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IPubSubSubscriberHandle> StartAsync(
            PubSubTaskConfig config,
            Func<PubSubPayload, CancellationToken, Task> handleMessageAsync,
            CancellationToken cancellationToken = default);
    }
}