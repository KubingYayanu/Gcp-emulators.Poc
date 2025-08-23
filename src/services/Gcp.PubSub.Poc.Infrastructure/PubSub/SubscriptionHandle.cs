using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;

namespace Gcp.PubSub.Poc.Infrastructure.PubSub
{
    public class SubscriptionHandle : ISubscriptionHandle
    {
        private readonly IPubSubSubscriberPool _subscriberPool;
        private readonly SubscriberClient _subscriber;
        private readonly ILogger _logger;
        private volatile bool _disposed;

        public string ConsumerId { get; }
        public string ProjectId { get; }
        public string SubscriptionId { get; }
        public Task StartTask { get; }

        public bool IsRunning =>
            StartTask is { IsCompleted: false, IsCanceled: false, IsFaulted: false };

        public SubscriptionHandle(
            IPubSubSubscriberPool subscriberPool,
            SubscriberClient subscriber,
            Task startTask,
            string consumerId,
            PubSubTaskConfig config,
            ILogger logger)
        {
            _subscriberPool = subscriberPool;
            _subscriber = subscriber;
            StartTask = startTask;
            ConsumerId = consumerId;
            ProjectId = config.ProjectId;
            SubscriptionId = config.SubscriptionId;
            _logger = logger;

            // 監控背景任務的異常
            _ = MonitorStartTaskAsync();
        }

        private async Task MonitorStartTaskAsync()
        {
            try
            {
                await StartTask;
                _logger.LogInformation(
                    "Subscription completed for consumer {ConsumerId}, subscription {ProjectId}:{SubscriptionId}",
                    ConsumerId,
                    ProjectId,
                    SubscriptionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Subscription failed for consumer {ConsumerId}, subscription {ProjectId}:{SubscriptionId}",
                    ConsumerId,
                    ProjectId,
                    SubscriptionId);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed) return;

            try
            {
                _logger.LogInformation(
                    "Stopping subscription for consumer {ConsumerId}, subscription {ProjectId}:{SubscriptionId}",
                    ConsumerId,
                    ProjectId,
                    SubscriptionId);

                // 停止 subscriber
                await _subscriber.StopAsync(TimeSpan.FromSeconds(30));

                // 清理 pool 中的資源
                await _subscriberPool.RemoveSubscriberAsync(ConsumerId, ProjectId, SubscriptionId);

                _logger.LogInformation(
                    "Stopped subscription for consumer {ConsumerId}, subscription {ProjectId}:{SubscriptionId}",
                    ConsumerId,
                    ProjectId,
                    SubscriptionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping subscription for consumer {ConsumerId}", ConsumerId);
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
                _logger.LogError(ex, "Error waiting for subscription completion");
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