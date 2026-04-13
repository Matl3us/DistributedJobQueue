using JobQueue.Core.Interfaces.Repositories;
using JobQueue.Core.Models.Entities;
using JobQueue.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace JobQueue.Infrastructure.Repositories;

public class OutboxRepository(JobContext context) : IOutboxRepository
{
    public void Add(Guid jobId, string payload)
    {
        var outbox = new Outbox
        {
            Payload = payload,
            JobId = jobId
        };
        context.Outbox.Add(outbox);
    }

    public async Task<IEnumerable<Outbox>> GetUnpublishedAsync(int amount)
    {
        return await context.Outbox.Where(o => o.Published == false)
            .Take(amount)
            .ToListAsync();
    }

    public void MarkPublished(Outbox outbox)
    {
        outbox.Published = true;
    }

    public void Reset(Guid jobId)
    {
        var outbox = context.Outbox.SingleOrDefault(o => o.JobId == jobId);
        if (outbox is null) return;
        outbox.Published = false;
        outbox.CreatedAt = DateTime.UtcNow;
    }
}