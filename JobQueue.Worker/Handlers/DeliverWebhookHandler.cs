using JobQueue.Core.Interfaces;
using JobQueue.Core.Models;
using JobQueue.Core.Models.DTOs.JobPayloads;

namespace JobQueue.Worker.Handlers;

public class DeliverWebhookHandler : IJobHandler<DeliverWebhookPayload>
{
    public async Task<JobResult> HandleAsync(DeliverWebhookPayload payload, CancellationToken ct)
    {
        await Task.Delay(3000, ct);
        return JobResult.Success("Webhook payload");
    }
}