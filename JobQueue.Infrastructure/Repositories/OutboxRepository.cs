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

    public async Task<IEnumerable<Outbox>> GetUnpublished()
    {
        return await context.Outbox.Where(o => o.Published == false)
            .ToListAsync();
    }

    public void MarkPublished(Outbox outbox)
    {
        outbox.Published = true;
    }
}