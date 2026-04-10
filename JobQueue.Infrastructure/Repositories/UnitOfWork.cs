using JobQueue.Core.Interfaces;
using JobQueue.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Storage;

namespace JobQueue.Infrastructure.Repositories;

public class UnitOfWork(JobContext context) : IUnitOfWork
{
    public async Task CommitAsync(CancellationToken ct = default)
    {
        await context.SaveChangesAsync(ct);
    }

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        var transaction = await context.Database.BeginTransactionAsync(ct);
        return new EfTransaction(transaction);
    }
}

public class EfTransaction(IDbContextTransaction transaction) : ITransaction
{
    public async Task CommitAsync(CancellationToken ct = default)
    {
        await transaction.CommitAsync(ct);
    }

    public async ValueTask DisposeAsync()
    {
        await transaction.DisposeAsync();
    }
}