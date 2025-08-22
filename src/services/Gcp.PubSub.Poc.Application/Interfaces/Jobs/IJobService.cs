using Gcp.PubSub.Poc.Domain.Enums;

namespace Gcp.PubSub.Poc.Application.Interfaces.Jobs
{
    public interface IJobService
    {
        JobType JobType { get; }

        Task RunAsync(CancellationToken cancellationToken);
    }
}