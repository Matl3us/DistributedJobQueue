using JobQueue.Core.Interfaces;
using JobQueue.Core.Models.DTOs;
using JobQueue.Core.Models.Entities;
using JobQueue.Core.Models.Enums;
using JobQueue.Infrastructure.Database;

namespace JobQueue.Infrastructure.Repositories;

public class JobRepository(JobContext context) : IJobRepository
{
    public async Task<Job> CreateJob(JobCreateDto jobCreateDto)
    {
        var job = new Job
        {
            Type = jobCreateDto.Type,
            Status = JobStatus.Pending,
            Payload = jobCreateDto.Payload
        };

        var result = await context.AddAsync(job);
        return result.Entity;
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}