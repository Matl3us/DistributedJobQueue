using System.ComponentModel;
using System.Text.Json;
using JobQueue.Core.Models.Entities;
using JobQueue.Core.Models.Enums;
using JobQueue.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace JobQueue.Worker.Workers;

public class JobProcessor(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<JobContext>();

            var timestamp = new NpgsqlParameter("timestamp", DateTime.UtcNow.AddMinutes(-10));
            var pendingJob = await context.Jobs
                .FromSql(
                    $"SELECT * FROM public.\"Jobs\" WHERE \"Status\" = 0 OR (\"Status\" = 1 AND \"UpdatedAt\" < {timestamp}) FOR UPDATE SKIP LOCKED LIMIT 1")
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
            catch (InvalidEnumArgumentException)
            {
                pendingJob.Status = JobStatus.Failed;

                Console.WriteLine($"Failed to process job: {pendingJob.Id}");
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
                await Task.Delay(10000);
                return JsonSerializer.Serialize(new { EmailSent = true });
            default:
                throw new InvalidEnumArgumentException("Invalid job type");
        }
    }
}