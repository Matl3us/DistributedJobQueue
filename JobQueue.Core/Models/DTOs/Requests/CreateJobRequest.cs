namespace JobQueue.Core.Models.DTOs.Requests;

public class CreateJobRequest
{
    public required string Type { get; init; }
    public required string Priority { get; init; }
    public string? Payload { get; init; }
}