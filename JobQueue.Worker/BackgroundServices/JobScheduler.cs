using JobQueue.Core.Interfaces;
using JobQueue.Core.Interfaces.Repositories;
using JobQueue.Core.Models;
using JobQueue.Core.Models.DTOs;
using JobQueue.Core.Models.Enums;
using System.Text.Json;

namespace JobQueue.Worker.BackgroundServices;

public class JobScheduler(IServiceProvider serviceProvider, ILogger<JobScheduler> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            var recurringJobRepository = scope.ServiceProvider.GetRequiredService<IRecurringJobRepository>();
            var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            try
            {
                await using var transaction = await unitOfWork.BeginTransactionAsync(stoppingToken);
                var recurringJob = await recurringJobRepository.GetDueAndLock();
                if (recurringJob is null)
                {
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }

                var jobCreate = new JobCreate()
                {
                    Type = recurringJob.Type,
                    Priority = JobPriority.High,
                    Payload = recurringJob.Payload,
                    RecurringJobId = recurringJob.Id
                };
                var job = jobRepository.Add(jobCreate);

                var message = new JobMessage()
                {
                    Id = job.Id,
                    Type = job.Type,
                    Priority = job.Priority,
                };
                var payload = JsonSerializer.Serialize(message);
                outboxRepository.Add(job.Id, payload);

                recurringJobRepository.UpdateNextRun(recurringJob);

                await unitOfWork.CommitAsync(stoppingToken);
                await transaction.CommitAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing recurring job");
            }
        }
    }
}