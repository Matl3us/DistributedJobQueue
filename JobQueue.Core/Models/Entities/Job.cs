using JobQueue.Core.Models.Enums;

namespace JobQueue.Core.Models.Entities;

public class Job
{
    public Guid Id { get; set; }
    public JobType Type { get; set; }
    public JobStatus Status { get; set; }
    public JobPriority Priority { get; set; }
    public string? Payload { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? Result { get; set; }
    public string? ErrorMessages { get; set; }
    public int RetryCount { get; set; }
    public DateTime NextRetryAt { get; set; }

    public DeadLetterJob? DeadLetterJob { get; set; }
}