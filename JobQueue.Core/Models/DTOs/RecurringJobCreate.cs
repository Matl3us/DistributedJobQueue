using JobQueue.Core.Models.Enums;

namespace JobQueue.Core.Models.DTOs;

public class RecurringJobCreate
{
    public required string Name { get; init; }
    public JobType Type { get; init; }
    public string? Payload { get; init; }
    public required string CronExpression { get; init; }
    public DateTime NextRun { get; init; }
}