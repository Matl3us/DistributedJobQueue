namespace JobQueue.Core.Models.DTOs.JobPayloads;

public record DeliverWebhookPayload
{
    public required Uri Url { get; init; }
    public required string Method { get; init; }
    public Dictionary<string, string> Headers { get; init; } = [];
    public string? Body { get; init; }
    public string? SigningSecret { get; init; }
}