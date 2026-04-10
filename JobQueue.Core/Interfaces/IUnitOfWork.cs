
namespace JobQueue.Core.Interfaces;

public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken ct = default);
    Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default);
}

public interface ITransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
}