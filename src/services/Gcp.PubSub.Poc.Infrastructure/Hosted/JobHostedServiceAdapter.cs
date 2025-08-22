using Gcp.PubSub.Poc.Application.Interfaces.Jobs;
using Microsoft.Extensions.Hosting;

namespace Gcp.PubSub.Poc.Infrastructure.Hosted
{
    public class JobHostedServiceAdapter : IHostedService
    {
        private readonly IJobService _job;
        private readonly IJobStopHandler? _stopHandler;

        public JobHostedServiceAdapter(
            IJobService job,
            IJobStopHandler? stopHandler)
        {
            _job = job;
            _stopHandler = stopHandler;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"[Adapter] Starting job: {_job.JobType}");
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

            Console.WriteLine($"[Adapter] No stop handler found for {_job.JobType}. Skipping cleanup.");
            return Task.CompletedTask;
        }
    }
}