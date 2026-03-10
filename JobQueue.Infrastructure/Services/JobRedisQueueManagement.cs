using System.ComponentModel;
using System.Text.Json;
using JobQueue.Core.Interfaces;
using JobQueue.Core.Models.DTOs;
using JobQueue.Core.Models.Enums;
using Microsoft.Extensions.Logging;
using NRedisStack;
using StackExchange.Redis;

namespace JobQueue.Infrastructure.Services;

public class JobRedisQueueManagement(
    IConnectionMultiplexer redis,
    ILogger<JobRedisQueueManagement> logger) : IJobRedisQueueManagement
{
    private readonly IDatabase _db = redis.GetDatabase();
    private readonly int _maxConsecutiveNoLowPrioJob = 5;
    private readonly string _processingQueue = "ProcessingQueue";

    private int _consecutiveNoLowPrioJobCounter;

    public async Task EnqueueAsync(Guid jobId, JobPriority priority)
    {
        var queue = ConvertToPriorityQueueName(priority);
        await _db.ListLeftPushAsync(queue, jobId.ToString());
        logger.LogInformation("Job {jobId} enqueued to queue: {queue}", jobId, queue);
        LogQueueDepth();
    }

    public async Task EnqueueOnTopAsync(Guid jobId, JobPriority priority)
    {
        var queue = ConvertToPriorityQueueName(priority);
        await _db.ListRightPushAsync(queue, jobId.ToString());
        logger.LogInformation("Job {jobId} enqueued on top to queue: {queue}", jobId, queue);
        LogQueueDepth();
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

        logger.LogInformation("Job {jobId} moved to the processing queue from: {queue}", jobId, queue);
        LogQueueDepth();

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
                await DequeueProcessingAsync(job);
                await _db.ListLeftPushAsync(job.Queue, job.Id.ToString());

                logger.LogInformation("Stuck job detected {jobId} and requeued.", job.Id);
                LogQueueDepth();
            }
        }
    }

    public async Task DequeueProcessingAsync(ProcessingJob job)
    {
        await _db.ListRemoveAsync(_processingQueue, JsonSerializer.Serialize(job));

        logger.LogInformation("Job {job} removed from processing queue", job);
        LogQueueDepth();
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

    private void LogQueueDepth()
    {
        var lowPrioDepth = _db.ListLength("queue:low");
        var normalPrioDepth = _db.ListLength("queue:normal");
        var highPrioDepth = _db.ListLength("queue:high");
        var processingDepth = _db.ListLength("ProcessingQueue");
        logger.LogInformation(
            $"Queue depth: [LowPriority]: {lowPrioDepth} [NormalPriority]: {normalPrioDepth} [HighPriority]: {highPrioDepth} [ProcessingQueue]: {processingDepth}");
    }
}