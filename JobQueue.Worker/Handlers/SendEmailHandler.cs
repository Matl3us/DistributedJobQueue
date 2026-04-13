using JobQueue.Core.Interfaces;
using JobQueue.Core.Models;
using JobQueue.Core.Models.DTOs.JobPayloads;

namespace JobQueue.Worker.Handlers;

public class SendEmailHandler : IJobHandler<SendEmailPayload>
{
    public async Task<JobResult> HandleAsync(SendEmailPayload payload, CancellationToken ct)
    {
        await Task.Delay(5000, ct);
        return JobResult.Success("Email payload");
    }
}