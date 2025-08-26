using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub.Publisher;
using Gcp.PubSub.Poc.Application.Interfaces.PubSub.Subscriber;
using Gcp.PubSub.Poc.Domain.Queues.Options;
using Gcp.PubSub.Poc.Infrastructure.PubSub;
using Gcp.PubSub.Poc.Infrastructure.PubSub.Publisher;
using Gcp.PubSub.Poc.Infrastructure.PubSub.Subscriber;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gcp.PubSub.Poc.Infrastructure.IoC
{
    public static class ConfigureInfrastructureServices
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            SetupQueues(services, configuration);
            AddPubSubHelper(services, configuration);

            return services;
        }

        private static void SetupQueues(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<WorkerQueueOptions>(options =>
            {
                configuration.GetSection(WorkerQueueOptions.SectionName).Bind(options);
            });
        }

        private static void AddPubSubHelper(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PubSubOptions>(options =>
            {
                configuration.GetSection(PubSubOptions.GcpPubSub).Bind(options);
            });

            // Producer
            services.AddSingleton<IPubSubPublisherPool, PubSubPublisherPool>();
            services.AddTransient<IPubSubPublisher, PubSubPublisher>();
            services.AddSingleton<IPubSubPublisherManager, PubSubPublisherManager>();

            // Consumer
            services.AddSingleton<IPubSubSubscriberPool, PubSubSubscriberPool>();
            services.AddTransient<IPubSubSubscriber, PubSubSubscriber>();
            services.AddSingleton<IPubSubSubscriberManager, PubSubSubscriberManager>();
        }
    }
}