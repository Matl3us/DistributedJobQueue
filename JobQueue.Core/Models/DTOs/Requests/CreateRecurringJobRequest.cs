namespace JobQueue.Core.Models.DTOs.Requests;

public class CreateRecurringJobRequest
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public string? Payload { get; init; }
    public required string CronExpression { get; init; }
}