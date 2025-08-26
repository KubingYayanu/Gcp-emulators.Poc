using Gcp.PubSub.Poc.Application.Interfaces.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gcp.PubSub.Poc.Application.IoC
{
    public static class ConfigureApplicationServices
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            SetupJobServices(services);
            SetupJobHandlers(services);
            SetupJobStopHandlers(services);

            return services;
        }

        private static void SetupJobServices(IServiceCollection services)
        {
            var jobServiceTypes = typeof(ConfigureApplicationServices).Assembly
                .DefinedTypes
                .Where(x => typeof(IJobService).IsAssignableFrom(x)
                            && !x.IsInterface);
            foreach (var type in jobServiceTypes)
            {
                services.AddTransient(typeof(IJobService), type);
            }
        }

        private static void SetupJobHandlers(IServiceCollection services)
        {
        }

        private static void SetupJobStopHandlers(IServiceCollection services)
        {
            var jobStopHandlerTypes = typeof(ConfigureApplicationServices).Assembly
                .DefinedTypes
                .Where(x => typeof(IJobStopHandler).IsAssignableFrom(x)
                            && !x.IsInterface);
            foreach (var type in jobStopHandlerTypes)
            {
                services.AddTransient(typeof(IJobStopHandler), type);
            }
        }
    }
}