using JobQueue.Core.Models;
using JobQueue.Core.Models.Enums;

namespace JobQueue.Core.Interfaces;

public interface IHandlerRegistry
{
    Task<JobResult> HandleAsync(JobType type, string payload, CancellationToken ct);
}