using JobQueue.Core.Models.DTOs;
using JobQueue.Core.Models.Entities;
using JobQueue.Core.Models.Enums;

namespace JobQueue.Core.Interfaces.Repositories;

public interface IJobRepository
{
    Job Add(JobCreate jobCreate);
    void AddError(JobError jobError);
    Task<Job> GetById(Guid jobId);
    void UpdateStatus(Job job, JobStatus status, string? result = null);
    Task<Dictionary<JobStatus, int>> GetCountByStatus();
    Task<IEnumerable<Job>> GetFailedPaginated(int page, int pageSize);
}