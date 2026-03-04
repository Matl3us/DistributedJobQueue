namespace JobQueue.Core.Models.DTOs;

public class ProcessingJob
{
    public Guid Id { get; init; }
    public required string Queue { get; init; }
    public DateTime StartedAt { get; init; }
}