using JobQueue.Infrastructure.Database;
using JobQueue.Worker.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace JobQueue.Worker.Workers;

public class DroppedJobsDetector(
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
            var deletedJob = await context.Jobs
                .FromSql(
                    $$$"""
                           SELECT * FROM public."Jobs" 
                           WHERE (("Status" = 1 
                            OR ("Status" = 3 AND "RetryCount" < {{{maxRetries}}} AND "NextRetryAt" <= {{{now}}})) 
                            AND "UpdatedAt" <= {{{timestamp}}})
                           FOR UPDATE SKIP LOCKED LIMIT 1
                       """)
                .FirstOrDefaultAsync(stoppingToken);

            if (deletedJob is null)
            {
                await Task.Delay(30000, stoppingToken);
                continue;
            }

            deletedJob.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(stoppingToken);
        }
    }
}