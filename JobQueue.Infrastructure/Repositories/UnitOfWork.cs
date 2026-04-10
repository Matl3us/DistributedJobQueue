using JobQueue.Core.Interfaces;
using JobQueue.Infrastructure.Database;

namespace JobQueue.Infrastructure.Repositories;

public class UnitOfWork(JobContext context) : IUnitOfWork
{
    public async Task CommitAsync(CancellationToken ct = default)
    {
        await context.SaveChangesAsync(ct);
    }
}