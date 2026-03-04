using JobQueue.Core.Models.DTOs;
using JobQueue.Core.Models.Enums;

namespace JobQueue.Core.Interfaces;

public interface IJobRedisQueueManagement
{
    Task EnqueueAsync(Guid jobId, JobPriority priority);
    Task<ProcessingJob?> MoveToProcessingAsync();
    Task CheckForStuckJobsAsync();
    Task DequeueProcessingAsync(ProcessingJob job);
}