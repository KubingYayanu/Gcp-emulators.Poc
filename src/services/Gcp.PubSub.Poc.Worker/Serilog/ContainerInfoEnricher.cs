using Serilog.Core;
using Serilog.Events;

namespace Gcp.PubSub.Poc.Worker.Serilog
{
    public class ContainerInfoEnricher : ILogEventEnricher
    {
        private readonly ContainerIdentity.ContainerInfo _containerInfo;

        public ContainerInfoEnricher(ContainerIdentity.ContainerInfo containerInfo)
        {
            _containerInfo = containerInfo;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var containerName = propertyFactory
                .CreateProperty("Container.Name", _containerInfo.ContainerName);
            var projectName = propertyFactory
                .CreateProperty("Container.ProjectName", _containerInfo.ProjectName);
            var ipAddress = propertyFactory
                .CreateProperty("Container.IpAddress", _containerInfo.IpAddress);

            logEvent.AddPropertyIfAbsent(containerName);
            logEvent.AddPropertyIfAbsent(projectName);
            logEvent.AddPropertyIfAbsent(ipAddress);
        }
    }
}