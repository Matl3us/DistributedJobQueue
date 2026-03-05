using System.ComponentModel;
using System.Text.Json;
using JobQueue.Core.Interfaces;
using JobQueue.Core.Models.DTOs;
using JobQueue.Core.Models.Enums;
using NRedisStack;
using StackExchange.Redis;

namespace JobQueue.Infrastructure.Services;

public class JobRedisQueueManagement(IConnectionMultiplexer redis) : IJobRedisQueueManagement
{
    private readonly IDatabase _db = redis.GetDatabase();
    private readonly int _maxConsecutiveNoLowPrioJob = 5;
    private readonly string _processingQueue = "ProcessingQueue";

    private int _consecutiveNoLowPrioJobCounter;

    public async Task EnqueueAsync(Guid jobId, JobPriority priority)
    {
        var queue = ConvertToPriorityQueueName(priority);
        await _db.ListLeftPushAsync(queue, jobId.ToString());
    }

    public async Task EnqueueOnTopAsync(Guid jobId, JobPriority priority)
    {
        var queue = ConvertToPriorityQueueName(priority);
        await _db.ListRightPushAsync(queue, jobId.ToString());
    }

    public async Task<ProcessingJob?> MoveToProcessingAsync()
    {
        var queue = GetQueueNameToCheck();
        var jobId = await _db.BRPopAsync(queue, 3);

        if (jobId is null)
            return null;

        var job = new ProcessingJob
        {
            Id = Guid.Parse(jobId.Item2.ToString()),
            Queue = queue,
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
                await _db.ListLeftPushAsync(job.Queue, job.Id.ToString());
            }
        }
    }

    public async Task DequeueProcessingAsync(ProcessingJob job)
    {
        await _db.ListRemoveAsync(_processingQueue, JsonSerializer.Serialize(job));
    }

    private string ConvertToPriorityQueueName(JobPriority priority)
    {
        return priority switch
        {
            JobPriority.Low => "queue:low",
            JobPriority.Normal => "queue:normal",
            JobPriority.High => "queue:high",
            _ => throw new InvalidEnumArgumentException()
        };
    }

    private string GetQueueNameToCheck()
    {
        if (_consecutiveNoLowPrioJobCounter == _maxConsecutiveNoLowPrioJob)
        {
            _consecutiveNoLowPrioJobCounter = 0;
            return "queue:low";
        }

        var rnd = Random.Shared;
        var num = rnd.Next(1, 7);
        if (num == 1)
        {
            _consecutiveNoLowPrioJobCounter = 0;
            return "queue:low";
        }

        _consecutiveNoLowPrioJobCounter++;
        if (num > 3) return "queue:high";
        return "queue:normal";
    }
}