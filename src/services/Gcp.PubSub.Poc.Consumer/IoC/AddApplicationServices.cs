using Gcp.PubSub.Poc.Consumer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gcp.PubSub.Poc.Consumer.IoC
{
    public static class ConfigureApplicationServices
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<IConsumerService, ConsumerService>();

            return services;
        }
    }
}