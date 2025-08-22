using CommandLine;
using Gcp.PubSub.Poc.Domain.Enums;

namespace Gcp.PubSub.Poc.Worker.Options
{
    public class JobOptions
    {
        [Option('j', "jobs", Required = true, HelpText = "Job names to run.", Separator = ' ')]
        public IEnumerable<JobType> Jobs { get; set; } = new List<JobType>();
    }
}