using Gcp.PubSub.Poc.Consumer.IoC;
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

            await host.StartAsync();
            await host.WaitForShutdownAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Console.WriteLine($"EnvironmentName: {environmentName}");

            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((host, builder) =>
                {
                    builder.SetBasePath(Directory.GetCurrentDirectory());
                    builder.AddJsonFile("appsettings.json", optional: true)
                        .AddJsonFile($"appsettings.{environmentName}.json", optional: true);
                    builder.AddCommandLine(args);
                })
                .UseSerilog((host, config) =>
                {
                    config.ReadFrom.Configuration(host.Configuration)
                        .Enrich.WithProperty("Service", "Gcp.PubSub.Poc.Consumer")
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