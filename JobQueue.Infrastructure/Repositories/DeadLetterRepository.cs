using JobQueue.Core.Interfaces.Repositories;
using JobQueue.Core.Models.Entities;
using JobQueue.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace JobQueue.Infrastructure.Repositories;

public class DeadLetterRepository(JobContext context) : IDeadLetterRepository
{
    public void Add(Guid jobId, string reason)
    {
        var deadLetterJob = new DeadLetterJob
        {
            Reason = reason,
            JobId = jobId
        };

        context.Add(deadLetterJob);
    }

    public async Task<IEnumerable<DeadLetterJob>> GetPaginated(int page, int pageSize)
    {
        return await context.DeadLetterJobs.Include(d => d.Job)
            .Skip(pageSize * (page - 1))
            .Take(pageSize)
            .ToListAsync();
    }

    public void Remove(DeadLetterJob deadLetterJob)
    {
        context.DeadLetterJobs.Remove(deadLetterJob);
    }
}