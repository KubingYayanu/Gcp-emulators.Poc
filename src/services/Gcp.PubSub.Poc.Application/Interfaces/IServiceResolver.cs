using Gcp.PubSub.Poc.Domain.Enums;

namespace Gcp.PubSub.Poc.Application.Interfaces
{
    public interface IServiceResolver
    {
        void RegisterJobs(IEnumerable<JobType> jobs);
    }
}