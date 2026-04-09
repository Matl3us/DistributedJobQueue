using JobQueue.Core.Models.Entities;

namespace JobQueue.Core.Interfaces.Repositories;

public interface IOutboxRepository
{
    Task Create(Guid jobId, string payload);
    Task<IEnumerable<Outbox>> GetUnpublished();
    Task MarkPublished(Guid outboxId);
}