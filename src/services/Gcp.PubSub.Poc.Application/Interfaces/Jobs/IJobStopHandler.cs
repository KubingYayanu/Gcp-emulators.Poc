using Gcp.PubSub.Poc.Domain.Enums;

namespace Gcp.PubSub.Poc.Application.Interfaces.Jobs
{
    public interface IJobStopHandler
    {
        JobType JobType { get; }

        Task StopAsync(CancellationToken cancellationToken);
    }
}