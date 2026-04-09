using JobQueue.Core.Models.DTOs;
using JobQueue.Core.Models.Entities;
using JobQueue.Core.Models.Enums;

namespace JobQueue.Core.Interfaces.Repositories;

public interface IJobRepository
{
    Task<Job> Create(JobCreate jobCreate);
    Task<Job> GetById(Guid jobId);
    Task UpdateStatus(Guid jobId, JobStatus status, string? result = null);
    Task<Dictionary<JobStatus, int>> GetCountByStatus();
    Task<IEnumerable<Job>> GetFailedPaginated(int page, int pageSize);
}