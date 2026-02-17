using JobQueue.Core.Models.DTOs;
using JobQueue.Core.Models.Entities;

namespace JobQueue.Core.Interfaces;

public interface IJobRepository
{
    Task<Job> CreateJob(JobCreateDto jobDto);
    Task SaveChangesAsync();
}