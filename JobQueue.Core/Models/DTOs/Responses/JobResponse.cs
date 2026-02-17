namespace JobQueue.Core.Models.DTOs.Responses;

public class JobResponse
{
    public Guid Id { get; init; }
    public required string Type { get; init; }
    public required string Status { get; init; }
    public string? Payload { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string? Result { get; init; }
}