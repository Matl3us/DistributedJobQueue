using JobQueue.Core.Models;

namespace JobQueue.Core.Interfaces;

public interface IJobHandler<TPayload>
{
    Task<JobResult> HandleAsync(TPayload payload, CancellationToken ct);
}