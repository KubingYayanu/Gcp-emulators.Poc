using Gcp.PubSub.Poc.Application.Interfaces.PubSub;
using Gcp.PubSub.Poc.Infrastructure.PubSub;
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
            services.Configure<PubSubOptions>(options =>
            {
                configuration.GetSection(PubSubOptions.GcpPubSub).Bind(options);
            });

            AddPubSubHelper(services);

            return services;
        }

        private static void AddPubSubHelper(IServiceCollection services)
        {
            // Producer
            services.AddSingleton<IPubSubPublisherPool, PubSubPublisherPool>();
            services.AddSingleton<IPubSubPublisher, PubSubPublisher>();

            // Consumer
            services.AddSingleton<IPubSubSubscriberPool, PubSubSubscriberPool>();
            services.AddTransient<IPubSubSubscriber, PubSubSubscriber>();
            services.AddSingleton<IPubSubSubscriptionManager, PubSubSubscriptionManager>();
        }
    }
}