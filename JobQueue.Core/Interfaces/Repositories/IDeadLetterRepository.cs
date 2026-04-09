using JobQueue.Core.Models.Entities;

namespace JobQueue.Core.Interfaces.Repositories;

public interface IDeadLetterRepository
{
    Task Add(Job job, string reason);
    Task<IEnumerable<DeadLetterJob>> GetPaginated(int page, int pageSize);
    Task Remove(Guid jobId);
}