using CommandLine;
using Gcp.PubSub.Poc.Application.Interfaces;
using Gcp.PubSub.Poc.Application.IoC;
using Gcp.PubSub.Poc.Infrastructure.IoC;
using Gcp.PubSub.Poc.Worker.Options;
using Gcp.PubSub.Poc.Worker.Resolvers;
using Gcp.PubSub.Poc.Worker.Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

class Program
{
    static async Task Main(string[] args)
    {
        JobOptions options = null;
        Parser.Default.ParseArguments<JobOptions>(args)
            .WithParsed(o => options = o)
            .WithNotParsed(errors =>
            {
                Console.WriteLine("Invalid command-line arguments.");
                Environment.Exit(1);
            });

        var host = CreateHostBuilder(options, args).Build();

        await host.StartAsync();
        await host.WaitForShutdownAsync();
    }

    private static IHostBuilder CreateHostBuilder(JobOptions options, string[] args)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

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
                var containerInfo = ContainerIdentity.GetContainerInfo();

                config.ReadFrom.Configuration(host.Configuration)
                    .Enrich.WithThreadId()
                    .Enrich.WithProperty("Service", "Gcp.PubSub.Poc.Worker")
                    .Enrich.With(new JobInfoEnricher(options))
                    .Enrich.With(new ContainerInfoEnricher(containerInfo))
                    .Enrich.FromLogContext()
                    .WriteTo.Console();

                var seqServerUrl = host.Configuration["Serilog:SeqServerUrl"];
                if (!string.IsNullOrWhiteSpace(seqServerUrl))
                {
                    config.WriteTo.Seq(seqServerUrl);
                }
            })
            .ConfigureServices((context, services) => ConfigureServices(context, services, options));
    }

    private static void ConfigureServices(
        HostBuilderContext context,
        IServiceCollection services,
        JobOptions options)
    {
        services.AddSingleton(options);
        services.AddSingleton<IServiceResolver, ServiceResolver>();

        var resolver = new ServiceResolver(services);
        resolver.RegisterJobs(options.Jobs);

        // Application
        services.AddInfrastructureServices(context.Configuration);

        // Infrastructure
        services.AddApplicationServices(context.Configuration);
    }
}