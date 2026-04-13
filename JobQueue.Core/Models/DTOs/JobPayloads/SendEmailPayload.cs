namespace JobQueue.Core.Models.DTOs.JobPayloads;

public record SendEmailPayload
{
    public required string Email { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
}