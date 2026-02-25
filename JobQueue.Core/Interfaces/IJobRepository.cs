using JobQueue.Core.Models.DTOs;
using JobQueue.Core.Models.Entities;
using JobQueue.Core.Models.Enums;

namespace JobQueue.Core.Interfaces;

public interface IJobRepository
{
    Task<Job> CreateJob(JobCreateDto jobDto);
    Task<Dictionary<JobStatus, int>> GetJobsCountByAllStatuses();
    Task<IEnumerable<Job>> GetFailedJobsPaginated(int page, int pageSize);
    Task<IEnumerable<DeadLetterJob>> GetDeadLetterQueueJobsPaginated(int page, int pageSize);
    Task SaveChangesAsync();
}