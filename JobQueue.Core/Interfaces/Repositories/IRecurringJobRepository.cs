using JobQueue.Core.Models.DTOs;
using JobQueue.Core.Models.Entities;

namespace JobQueue.Core.Interfaces.Repositories;

public interface IRecurringJobRepository
{
    Task<RecurringJob> Create(RecurringJobCreate recurringJobCreate);
    Task<IEnumerable<RecurringJob>> GetPaginated(int page, int pageSize);
    Task<RecurringJob?> GetDueAndUpdateNextRun();
}