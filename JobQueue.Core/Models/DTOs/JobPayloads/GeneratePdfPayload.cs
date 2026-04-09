namespace JobQueue.Core.Models.DTOs.JobPayloads;

public record GeneratePdfPayload
{
    public required string TemplateId { get; init; }
    public required string Data { get; init; }
}