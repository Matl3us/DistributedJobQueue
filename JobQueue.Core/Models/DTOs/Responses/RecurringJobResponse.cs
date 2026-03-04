namespace JobQueue.Core.Models.DTOs.Responses;

public class RecurringJobResponse
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Type { get; init; }
    public string? Payload { get; init; }
    public required string CronExpression { get; init; }
    public DateTime? LastRun { get; init; }
    public DateTime NextRun { get; init; }
}