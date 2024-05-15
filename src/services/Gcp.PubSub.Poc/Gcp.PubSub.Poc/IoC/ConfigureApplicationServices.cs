using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Redis.Poc.Services;

namespace Redis.Poc.IoC
{
    public static class ConfigureApplicationServices
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<IProducerService, ProducerService>();

            return services;
        }
    }
}