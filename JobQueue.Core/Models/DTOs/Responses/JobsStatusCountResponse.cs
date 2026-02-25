namespace JobQueue.Core.Models.DTOs.Responses;

public class JobsStatusCountResponse
{
    public int PendingJobsCount { get; init; }
    public int ProcessingJobsCount { get; init; }
    public int CompletedJobsCount { get; init; }
    public int FailedJobsCount { get; init; }
}