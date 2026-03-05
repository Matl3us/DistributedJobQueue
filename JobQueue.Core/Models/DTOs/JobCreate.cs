using JobQueue.Core.Models.Enums;

namespace JobQueue.Core.Models.DTOs;

public class JobCreate
{
    public JobType Type { get; init; }
    public JobPriority Priority { get; init; }
    public string? Payload { get; init; }
    public Guid? RecurringJobId { get; init; }
}