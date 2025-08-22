using Gcp.PubSub.Poc.Application.Interfaces;
using Gcp.PubSub.Poc.Application.Interfaces.Jobs;
using Gcp.PubSub.Poc.Domain.Enums;
using Gcp.PubSub.Poc.Infrastructure.Hosted;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Gcp.PubSub.Poc.Worker.Resolvers
{
    public class ServiceResolver : IServiceResolver
    {
        private readonly IServiceCollection _services;

        public ServiceResolver(IServiceCollection services)
        {
            _services = services;
        }

        public void RegisterJobs(IEnumerable<JobType> jobs)
        {
            foreach (var job in jobs)
            {
                // 避免使用 .AddHostedService<T>()，它是一個簡化方法，本質上會呼叫：
                // services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, T>());
                // 這個 TryAddEnumerable 會忽略重複註冊的 IHostedService (即使是 lambda)
                _services.AddTransient<IHostedService>(provider =>
                {
                    var allJobs = provider.GetServices<IJobService>();
                    var stopHandlers = provider.GetServices<IJobStopHandler>();

                    var jobInstance = allJobs.FirstOrDefault(x => x.JobType == job);
                    if (jobInstance == null)
                        throw new NotSupportedException($"Not supported job type: {job}");

                    var stopHandler = stopHandlers.FirstOrDefault(x => x.JobType == job);

                    return new JobHostedServiceAdapter(jobInstance, stopHandler);
                });
            }
        }
    }
}