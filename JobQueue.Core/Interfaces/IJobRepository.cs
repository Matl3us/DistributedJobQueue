using JobQueue.Core.Models.DTOs.Requests;
using JobQueue.Core.Models.Entities;

namespace JobQueue.Core.Interfaces;

public interface IJobRepository
{
    Task<Job> CreateJob(CreateJobRequest request);
    Task SaveChangesAsync();
}