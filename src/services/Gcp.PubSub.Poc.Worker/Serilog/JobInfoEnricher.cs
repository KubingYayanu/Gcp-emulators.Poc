using Gcp.PubSub.Poc.Worker.Options;
using Serilog.Core;
using Serilog.Events;

namespace Gcp.PubSub.Poc.Worker.Serilog
{
    public class JobInfoEnricher : ILogEventEnricher
    {
        private readonly JobOptions _jobOptions;

        public JobInfoEnricher(JobOptions jobOptions)
        {
            _jobOptions = jobOptions;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var enabledJobs = _jobOptions.Jobs.Select(x => x.ToString());
            var property = propertyFactory.CreateProperty("EnabledJobs", enabledJobs);
            logEvent.AddPropertyIfAbsent(property);
        }
    }
}