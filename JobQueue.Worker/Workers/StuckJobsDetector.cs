using JobQueue.Core.Interfaces;

namespace JobQueue.Worker.Workers;

public class StuckJobsDetector(IJobRedisQueueManagement redisQueue) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await redisQueue.CheckForStuckJobsAsync();
            await Task.Delay(30000, stoppingToken);
        }
    }
}