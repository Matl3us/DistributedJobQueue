using JobQueue.Core.Models.DTOs;
using JobQueue.Core.Models.Entities;
using JobQueue.Core.Models.Enums;

namespace JobQueue.Core.Interfaces;

public interface IJobRepository
{
    Task<Job> CreateJob(JobCreateDto jobDto);
    Task<Dictionary<JobStatus, int>> GetJobsCountByAllStatuses();
    Task SaveChangesAsync();
}