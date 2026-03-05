using JobQueue.Core.Interfaces;
using JobQueue.Core.Models.DTOs.Requests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JobQueue.Infrastructure.Worker;

public class JobScheduler(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var jobService = scope.ServiceProvider.GetRequiredService<IJobManagementService>();

            var scheduledJob = await jobService.GetNextScheduledJob();
            if (scheduledJob is null)
            {
                await Task.Delay(5000, stoppingToken);
                continue;
            }

            Console.WriteLine($"Processing recurring job with id: {scheduledJob.Id}");

            var jobRequest = new CreateJobFromRecurringRequest
            {
                Type = scheduledJob.Type,
                Payload = scheduledJob.Payload,
                RecurringJobId = scheduledJob.Id
            };
            await jobService.CreateJobFromRecurring(jobRequest);

            await jobService.CalculateNextRunForJob(scheduledJob.Id);
        }
    }
}