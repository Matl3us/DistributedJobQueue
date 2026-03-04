using JobQueue.Core.Models.Enums;

namespace JobQueue.Core.Models.DTOs;

public class JobCreateDto
{
    public JobType Type { get; init; }
    public JobPriority Priority { get; init; }
    public string? Payload { get; init; }
}