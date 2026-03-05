using JobQueue.Core.Models.Enums;

namespace JobQueue.Core.Models.Entities;

public class RecurringJob
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public JobType Type { get; set; }
    public string? Payload { get; set; }
    public required string CronExpression { get; set; }
    public DateTime? LastRun { get; set; }
    public DateTime? NextRun { get; set; }

    public ICollection<Job> Jobs { get; } = new List<Job>();
}