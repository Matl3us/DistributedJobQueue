using JobQueue.Core.Interfaces;
using JobQueue.Core.Models.DTOs.Requests;
using JobQueue.Core.Models.Entities;
using JobQueue.Core.Models.Enums;
using JobQueue.Infrastructure.Database;

namespace JobQueue.Infrastructure.Repositories;

public class JobRepository(JobContext context) : IJobRepository
{
    public async Task<Job> CreateJob(CreateJobRequest request)
    {
        var job = new Job
        {
            Type = request.Type,
            Status = JobStatus.Pending,
            Payload = request.Payload
        };

        var result = await context.AddAsync(job);
        return result.Entity;
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}