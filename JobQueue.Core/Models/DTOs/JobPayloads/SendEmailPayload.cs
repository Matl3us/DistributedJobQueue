namespace JobQueue.Core.Models.DTOs.JobPayloads;

public class SendEmailPayload
{
    public required string Email { get; init; }
    public required string Topic { get; init; }
    public required string Content { get; init; }
}