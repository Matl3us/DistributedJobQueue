using JobQueue.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JobQueue.Application.Workers;

public class JobScheduler(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var jobService = scope.ServiceProvider.GetRequiredService<IJobManagementService>();

            var result = await jobService.ScheduleRecurringJob();
            if (!result) await Task.Delay(3000, stoppingToken);
        }
    }
}