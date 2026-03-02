using System.Text.Json;
using JobQueue.Core.Interfaces;
using JobQueue.Core.Models.DTOs;
using StackExchange.Redis;

namespace JobQueue.Infrastructure.Services;

public class JobRedisQueueManagement(IConnectionMultiplexer redis) : IJobRedisQueueManagement
{
    private readonly IDatabase _db = redis.GetDatabase();
    private readonly string _processingQueue = "ProcessingQueue";
    private readonly string _queue = "queue";

    public async Task EnqueueAsync(Guid jobId)
    {
        await _db.ListLeftPushAsync(_queue, jobId.ToString());
    }

    public async Task<ProcessingJob?> MoveToProcessingAsync()
    {
        var jobId = await _db.ListRightPopAsync(_queue);

        if (jobId.IsNullOrEmpty)
            return null;

        var job = new ProcessingJob
        {
            Id = Guid.Parse(jobId.ToString()),
            StartedAt = DateTime.UtcNow
        };

        var jobJson = JsonSerializer.Serialize(job);
        await _db.ListLeftPushAsync(_processingQueue, jobJson);

        return job;
    }

    public async Task CheckForStuckJobsAsync()
    {
        var items = await _db.ListRangeAsync(_processingQueue);

        foreach (var item in items)
        {
            var job = JsonSerializer.Deserialize<ProcessingJob>(item.ToString());

            if (job?.StartedAt < DateTime.UtcNow.AddMinutes(-10))
            {
                Console.WriteLine($"Requeuing stuck job: {job.Id}");

                await DequeueProcessingAsync(job);
                await _db.ListLeftPushAsync(_queue, job.Id.ToString());
            }
        }
    }

    public async Task DequeueProcessingAsync(ProcessingJob job)
    {
        await _db.ListRemoveAsync(_processingQueue, JsonSerializer.Serialize(job));
    }
}