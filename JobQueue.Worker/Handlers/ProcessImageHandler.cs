using JobQueue.Core.Interfaces;
using JobQueue.Core.Models;
using JobQueue.Core.Models.DTOs.JobPayloads;

namespace JobQueue.Worker.Handlers;

public class ProcessImageHandler : IJobHandler<ProcessImagePayload>
{
    public async Task<JobResult> HandleAsync(ProcessImagePayload payload, CancellationToken ct)
    {
        await Task.Delay(6000, ct);
        return JobResult.Success("Image payload");
    }
}