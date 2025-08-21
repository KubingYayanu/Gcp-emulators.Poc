namespace Gcp.PubSub.Poc.Helpers.V2
{
    public interface IPubSubConsumer
    {
        Task StartAsync(
            PubSubTaskConfig config,
            Func<PubSubPayload, CancellationToken, Task> handleMessageAsync,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// 手動調用 StopAsync 方法來停止消費者
        /// 配合業務邏輯使用，依據需求決定何時停止消費者
        /// 與 Disposable 不同，這個方法不會自動調用
        /// </summary>
        /// <param name="config"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StopAsync(
            PubSubTaskConfig config,
            CancellationToken cancellationToken = default);
    }
}