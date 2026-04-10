using JobQueue.Core.Models.Entities;

namespace JobQueue.Core.Interfaces.Repositories;

public interface IOutboxRepository
{
    void Add(Guid jobId, string payload);
    Task<IEnumerable<Outbox>> GetUnpublished();
    void MarkPublished(Outbox outbox);
}