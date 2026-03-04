namespace JobQueue.Core.Models.DTOs.Responses;

public class FailedJobResponse
{
    public Guid Id { get; init; }
    public required string Type { get; init; }
    public required string Priority { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string? ErrorMessage { get; init; }
}