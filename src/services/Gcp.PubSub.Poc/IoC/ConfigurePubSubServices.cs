using Gcp.PubSub.Poc.Helpers;
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

            return services;
        }
    }
}