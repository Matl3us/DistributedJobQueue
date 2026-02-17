using JobQueue.Core.Models.Enums;

namespace JobQueue.Core.Models.DTOs.Responses;

public class JobDto
{
    public Guid Id { get; init; }
    public JobType Type { get; init; }
    public JobStatus Status { get; init; }
    public string? Payload { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string? Result { get; init; }
}