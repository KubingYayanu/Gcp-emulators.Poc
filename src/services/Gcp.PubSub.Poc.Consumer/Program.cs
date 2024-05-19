using Gcp.PubSub.Poc.Consumer.IoC;
using Gcp.PubSub.Poc.Consumer.Services;
using Gcp.PubSub.Poc.IoC;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Gcp.PubSub.Poc.Consumer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            var consumer = host.Services.GetRequiredService<IConsumerService>();
            await consumer.PullMessagesAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((host, builder) =>
                {
                    builder.SetBasePath(Directory.GetCurrentDirectory());
                    builder.AddJsonFile("appsettings.json", optional: true);
                    builder.AddCommandLine(args);
                })
                .UseSerilog((host, config) =>
                {
                    config.ReadFrom.Configuration(host.Configuration)
                        .Enrich.FromLogContext()
                        .WriteTo.Console();
                })
                .ConfigureServices(ConfigureServices);
        }

        private static void ConfigureServices(HostBuilderContext host, IServiceCollection services)
        {
            services.AddPubSubResourceHelper(host.Configuration);
            services.AddApplicationServices(host.Configuration);
        }
    }
}