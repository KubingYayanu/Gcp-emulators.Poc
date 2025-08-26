using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;

namespace Gcp.PubSub.Poc.Infrastructure.PubSub
{
    public class PubSubSubscriberHandle : IPubSubSubscriberHandle
    {
        private readonly IPubSubSubscriberPool _subscriberPool;
        private readonly SubscriberClient _subscriber;
        private readonly ILogger _logger;
        private volatile bool _disposed;

        public PubSubSubscriberHandle(
            string subscriberId,
            PubSubTaskConfig config,
            IPubSubSubscriberPool subscriberPool,
            SubscriberClient subscriber,
            Task startTask,
            ILogger logger)
        {
            _subscriberPool = subscriberPool;
            _subscriber = subscriber;
            StartTask = startTask;
            SubscriberId = subscriberId;
            ProjectId = config.ProjectId;
            SubscriptionId = config.SubscriptionId;
            _logger = logger;

            // 監控背景任務的異常
            _ = MonitorStartTaskAsync();
        }

        public string SubscriberId { get; }

        public string ProjectId { get; }

        public string SubscriptionId { get; }

        public Task StartTask { get; }

        public bool IsRunning =>
            StartTask is { IsCompleted: false, IsCanceled: false, IsFaulted: false };

        private async Task MonitorStartTaskAsync()
        {
            try
            {
                await StartTask;
                _logger.LogInformation(
                    message: "Subscription completed for Subscriber {SubscriberId}, "
                             + "ProjectId: {ProjectId}, SubscriptionId: {SubscriptionId}",
                    args:
                    [
                        SubscriberId,
                        ProjectId,
                        SubscriptionId
                    ]);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    exception: ex,
                    message: "Subscription failed for Subscriber {SubscriberId}, "
                             + "ProjectId: {ProjectId}, SubscriptionId: {SubscriptionId}",
                    args:
                    [
                        SubscriberId,
                        ProjectId,
                        SubscriptionId
                    ]);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed) return;

            try
            {
                _logger.LogInformation(
                    message: "Stopping subscription for Subscriber {SubscriberId}, "
                             + "ProjectId: {ProjectId}, SubscriptionId: {SubscriptionId}",
                    args:
                    [
                        SubscriberId,
                        ProjectId,
                        SubscriptionId
                    ]);

                // 停止 subscriber
                await _subscriber.StopAsync(TimeSpan.FromSeconds(30));

                // 清理 pool 中的資源
                await _subscriberPool.RemoveSubscriberAsync(
                    subscriberId: SubscriberId,
                    projectId: ProjectId,
                    subscriptionId: SubscriptionId,
                    cancellationToken: cancellationToken);

                _logger.LogInformation(
                    message: "Stopped subscription for Subscriber {SubscriberId}, "
                             + "ProjectId: {ProjectId}, SubscriptionId: {SubscriptionId}",
                    args:
                    [
                        SubscriberId,
                        ProjectId,
                        SubscriptionId
                    ]);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    exception: ex,
                    message: "Error stopping subscription for Subscriber {SubscriberId}"
                             + "ProjectId: {ProjectId}, SubscriptionId: {SubscriptionId}",
                    args:
                    [
                        SubscriberId,
                        ProjectId,
                        SubscriptionId
                    ]);
                throw;
            }
        }

        public async Task WaitForCompletionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await StartTask.WaitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    exception: ex,
                    message: "Error waiting for subscription completion. Subscriber {SubscriberId}, "
                             + "ProjectId: {ProjectId}, SubscriptionId: {SubscriptionId}",
                    args:
                    [
                        SubscriberId,
                        ProjectId,
                        SubscriptionId
                    ]);
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            await StopAsync();
        }
    }
}