using JobQueue.Core.Models.DTOs;
using JobQueue.Core.Models.Entities;

namespace JobQueue.Core.Interfaces.Repositories;

public interface IRecurringJobRepository
{
    RecurringJob Add(RecurringJobCreate recurringJobCreate);
    Task<IEnumerable<RecurringJob>> GetPaginated(int page, int pageSize);
    Task<RecurringJob?> GetDueAndLock();
    void UpdateNextRun(RecurringJob recurringJob);
    void Remove(RecurringJob recurringJob);
}