using JobQueue.Core.Interfaces;
using JobQueue.Core.Interfaces.Repositories;
using JobQueue.Core.Models;
using JobQueue.Core.Models.Enums;
using JobQueue.Worker.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace JobQueue.Worker.BackgroundServices;

public class RetryScheduler(IServiceProvider serviceProvider,
    ILogger<RetryScheduler> logger,
    IOptions<WorkerOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var maxRetries = options.Value.MaxJobRetries;

        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            try
            {
                var job = await jobRepository.GetDueForRetryAndLock(maxRetries);
                if (job is null)
                {
                    await Task.Delay(5000, stoppingToken);
                    continue;
                }

                var message = new JobMessage()
                {
                    Id = job.Id,
                    Type = job.Type,
                    Priority = job.Priority,
                };
                var payload = JsonSerializer.Serialize(message);
                outboxRepository.Add(job.Id, payload);

                job.Status = JobStatus.Pending;
                job.NextRetryAt = null;

                await unitOfWork.CommitAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrying to schedule job");
            }
        }
    }
}