using JobQueue.Core.Models.Enums;

namespace JobQueue.Core.Interfaces;

public interface IHandlerRegistry
{
    Task HandleAsync(JobType type, string payload, CancellationToken ct);
}