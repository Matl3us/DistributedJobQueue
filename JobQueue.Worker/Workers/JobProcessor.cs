using System.ComponentModel;
using System.Text.Json;
using JobQueue.Core.Models.DTOs.JobPayloads;
using JobQueue.Core.Models.Entities;
using JobQueue.Core.Models.Enums;
using JobQueue.Infrastructure.Database;
using JobQueue.Worker.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace JobQueue.Worker.Workers;

public class JobProcessor(
    IServiceProvider serviceProvider,
    IOptions<WorkerOptions> options) : BackgroundService
{
    private readonly int _maxJobRetries = options.Value.MaxJobRetries;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<JobContext>();

            var timestamp = new NpgsqlParameter("timestamp", DateTime.UtcNow.AddMinutes(-10));
            var now = new NpgsqlParameter("now", DateTime.UtcNow);
            var maxRetries = new NpgsqlParameter("maxRetries", _maxJobRetries);
            var pendingJob = await context.Jobs
                .FromSql(
                    $$"""
                          SELECT * FROM public."Jobs" 
                          WHERE ("Status" = 0
                             OR ("Status" = 3 AND "RetryCount" < {{maxRetries}} AND "NextRetryAt" <= {{now}})
                             OR ("Status" = 1 AND "UpdatedAt" <= {{timestamp}}))
                          FOR UPDATE SKIP LOCKED LIMIT 1
                      """)
                .FirstOrDefaultAsync(stoppingToken);

            if (pendingJob is null)
            {
                await Task.Delay(5000, stoppingToken);
                continue;
            }

            Console.WriteLine($"Processing job: {pendingJob.Id}");

            pendingJob.Status = JobStatus.Processing;
            pendingJob.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(stoppingToken);


            try
            {
                var result = await HandleJob(pendingJob);
                pendingJob.Status = JobStatus.Completed;
                pendingJob.Result = result;

                Console.WriteLine($"Successfully processed job: {pendingJob.Id}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to process job: {pendingJob.Id}\nError message: {e.Message}");
                
                pendingJob.ErrorMessages += $"Attempt {pendingJob.RetryCount + 1}: {e.Message}\n";
                pendingJob.Status = JobStatus.Failed;

                if (pendingJob.RetryCount < _maxJobRetries)
                {
                    pendingJob.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, pendingJob.RetryCount));
                    pendingJob.RetryCount++;

                    Console.WriteLine($"Next retry for job: {pendingJob.Id} set at {pendingJob.NextRetryAt}");
                }
            }
            finally
            {
                pendingJob.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(stoppingToken);
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

                try
                {
                    JsonSerializer.Deserialize<SendEmailPayload>(job.Payload);
                }
                catch (Exception)
                {
                    throw new JsonException("Invalid json format for the sending email payload job");
                }

                await Task.Delay(10000);
                return JsonSerializer.Serialize(new { EmailSent = true });
            default:
                throw new InvalidEnumArgumentException("Invalid job type");
        }
    }
}