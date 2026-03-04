using System.ComponentModel;
using System.Text.Json;
using JobQueue.Core.Interfaces;
using JobQueue.Core.Models.DTOs.JobPayloads;
using JobQueue.Core.Models.Entities;
using JobQueue.Core.Models.Enums;
using JobQueue.Infrastructure.Database;
using JobQueue.Worker.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JobQueue.Worker.Workers;

public class JobProcessor(
    IServiceProvider serviceProvider,
    IJobRedisQueueManagement redisQueue,
    IOptions<WorkerOptions> options) : BackgroundService
{
    private readonly int _maxJobRetries = options.Value.MaxJobRetries;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<JobContext>();

            var pendingJob = await redisQueue.MoveToProcessingAsync();

            if (pendingJob is null)
                continue;

            var job = await context.Jobs.SingleAsync(j => j.Id == pendingJob.Id, stoppingToken);

            Console.WriteLine($"Processing job: {job.Id}");

            job.Status = JobStatus.Processing;
            job.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(stoppingToken);

            try
            {
                var result = await HandleJob(job);
                job.Status = JobStatus.Completed;
                job.Result = result;

                Console.WriteLine($"Successfully processed job: {job.Id}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to process job: {job.Id}\nError message: {e.Message}");

                job.ErrorMessages += $"Attempt {job.RetryCount + 1}: {e.Message}\n";
                job.Status = JobStatus.Failed;
                job.RetryCount++;

                if (job.RetryCount < _maxJobRetries)
                {
                    job.NextRetryAt = DateTime.UtcNow.AddSeconds(Math.Pow(2, job.RetryCount));
                    await redisQueue.EnqueueAsync(job.Id, job.Priority);

                    Console.WriteLine($"Next retry for job: {job.Id} set at {job.NextRetryAt}");
                }
                else
                {
                    Console.WriteLine($"Moving job: {job.Id} to dead letter jobs table");

                    var deadLetterJob = new DeadLetterJob
                    {
                        Job = job,
                        Reason = "Max number of retries reached"
                    };
                    await context.DeadLetterJobs.AddAsync(deadLetterJob, stoppingToken);
                }
            }
            finally
            {
                job.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(stoppingToken);
                await redisQueue.DequeueProcessingAsync(pendingJob);
            }
        }
    }

    private async Task<string?> HandleJob(Job job)
    {
        switch (job.Type)
        {
            case JobType.SendEmail:

                if (job.Payload is null)
                    throw new ArgumentNullException(nameof(job.Payload),
                        "Job payload for sending email cannot be empty");

                await Task.Delay(10000);
                try
                {
                    JsonSerializer.Deserialize<SendEmailPayload>(job.Payload);
                }
                catch (Exception)
                {
                    throw new JsonException("Invalid json format for the sending email payload job");
                }

                return JsonSerializer.Serialize(new { EmailSent = true });
            default:
                throw new InvalidEnumArgumentException("Invalid job type");
        }
    }
}