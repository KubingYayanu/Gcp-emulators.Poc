using Gcp.PubSub.Poc.Application.Interfaces.Jobs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gcp.PubSub.Poc.Infrastructure.Hosted
{
    public class JobHostedServiceAdapter : IHostedService
    {
        private readonly IJobService _job;
        private readonly IJobStopHandler? _stopHandler;
        private readonly ILogger<JobHostedServiceAdapter> _logger;

        public JobHostedServiceAdapter(
            IJobService job,
            IJobStopHandler? stopHandler,
            ILogger<JobHostedServiceAdapter> logger)
        {
            _job = job;
            _stopHandler = stopHandler;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                message: "[Adapter] Starting job: {JobType}",
                args: _job.JobType);

            _job.RunAsync(cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            var handler = _stopHandler;
            if (handler != null)
            {
                return handler.HandleStopAsync(_job, cancellationToken);
            }

            _logger.LogInformation(
                message: "[Adapter] No stop handler found for {JobType}. Skipping cleanup",
                args: _job.JobType);
            return Task.CompletedTask;
        }
    }
}