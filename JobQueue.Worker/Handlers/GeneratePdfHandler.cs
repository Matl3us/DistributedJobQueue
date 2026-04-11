using JobQueue.Core.Interfaces;
using JobQueue.Core.Models;
using JobQueue.Core.Models.DTOs.JobPayloads;

namespace JobQueue.Worker.Handlers;

public class GeneratePdfHandler : IJobHandler<GeneratePdfPayload>
{
    public async Task<JobResult> HandleAsync(GeneratePdfPayload payload, CancellationToken ct)
    {
        await Task.Delay(8000, ct);
        return JobResult.Success("Pdf payload");
    }
}