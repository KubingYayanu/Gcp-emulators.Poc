using Gcp.PubSub.Poc.Domain.Queues.Options;
using Gcp.PubSub.Poc.Helpers;
using Gcp.PubSub.Poc.Helpers.V2;
using Gcp.PubSub.Poc.Helpers.V3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IPubSubConsumer_V3 = Gcp.PubSub.Poc.Helpers.V3.IPubSubConsumer;
using IPubSubSubscriberPool_V3 = Gcp.PubSub.Poc.Helpers.V3.IPubSubSubscriberPool;
using PubSubConsumer_V3 = Gcp.PubSub.Poc.Helpers.V3.PubSubConsumer;
using PubSubSubscriberPool_V3 = Gcp.PubSub.Poc.Helpers.V3.PubSubSubscriberPool;

namespace Gcp.PubSub.Poc.IoC
{
    public static class ConfigurePubSubServices
    {
        public static IServiceCollection AddPubSubResourceHelper(
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
            services.AddSingleton<IPubSubPublisher, PubSubPublisher>();

            // Consumer
            services.AddSingleton<IPubSubSubscriberPool_V3, PubSubSubscriberPool_V3>();
            services.AddTransient<IPubSubConsumer_V3, PubSubConsumer_V3>();
            services.AddSingleton<IPubSubSubscriptionManager, PubSubSubscriptionManager>();
        }
    }
}