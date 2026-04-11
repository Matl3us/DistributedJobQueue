using JobQueue.Core.Models.Entities;

namespace JobQueue.Core.Interfaces.Repositories;

public interface IOutboxRepository
{
    void Add(Guid jobId, string payload);
    Task<IEnumerable<Outbox>> GetUnpublishedAsync(int amount);
    void MarkPublished(Outbox outbox);
}