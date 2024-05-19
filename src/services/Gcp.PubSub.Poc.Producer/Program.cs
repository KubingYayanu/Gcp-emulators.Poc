using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Redis.Poc.IoC;
using Redis.Poc.Services;
using Serilog;

namespace Gcp.PubSub.Poc.Producer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            var producer = host.Services.GetRequiredService<IProducerService>();
            await producer.Run();
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
            services.AddApplicationServices(host.Configuration);
        }
    }
}