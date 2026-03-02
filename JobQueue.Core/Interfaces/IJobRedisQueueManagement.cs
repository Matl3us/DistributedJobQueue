using JobQueue.Core.Models.DTOs;

namespace JobQueue.Core.Interfaces;

public interface IJobRedisQueueManagement
{
    Task EnqueueAsync(Guid jobId);
    Task<ProcessingJob?> MoveToProcessingAsync();
    Task CheckForStuckJobsAsync();
    Task DequeueProcessingAsync(ProcessingJob job);
}