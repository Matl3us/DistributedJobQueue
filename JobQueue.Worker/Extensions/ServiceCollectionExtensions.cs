using JobQueue.Infrastructure.Messaging;
using JobQueue.Worker.BackgroundServices;

namespace JobQueue.Worker.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<OutboxProcessor>();
        services.AddHostedService<JobScheduler>();
    }
}