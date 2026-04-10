using JobQueue.Core.Models.Entities;

namespace JobQueue.Core.Interfaces.Repositories;

public interface IDeadLetterRepository
{
    void Add(Guid jobId, string reason);
    Task<IEnumerable<DeadLetterJob>> GetPaginated(int page, int pageSize);
    void Remove(DeadLetterJob deadLetterJob);
}