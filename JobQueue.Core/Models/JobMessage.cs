using JobQueue.Core.Models.Enums;

namespace JobQueue.Core.Models;

public record JobMessage
{
    public Guid Id { get; init; }
    public JobType Type { get; init; }
    public JobPriority Priority { get; init; }
}