using Gcp.PubSub.Poc.Helpers;
using Gcp.PubSub.Poc.Helpers.V1;
using Gcp.PubSub.Poc.Helpers.V2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Gcp.PubSub.Poc.IoC
{
    public static class ConfigurePubSubServices
    {
        public static IServiceCollection AddPubSubResourceHelper(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<PubSubOptions>(options =>
            {
                configuration.GetSection(PubSubOptions.GcpPubSub).Bind(options);
            });

            services.TryAddSingleton<IPubSubResourceHelper, PubSubResourceHelper>();

            AddPubSubHelper(services);

            return services;
        }

        private static void AddPubSubHelper(IServiceCollection services)
        {
            services.AddSingleton<IPubSubPublisherPool, PubSubPublisherPool>();
            services.AddSingleton<IPubSubSubscriberPool, PubSubSubscriberPool>();
            services.AddSingleton<IPubSubPublisher, PubSubPublisher>();
            services.AddSingleton<IPubSubConsumer, PubSubConsumer>();
        }
    }
}