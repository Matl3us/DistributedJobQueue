using JobQueue.Core.Interfaces;
using JobQueue.Core.Models.DTOs;
using JobQueue.Core.Models.Entities;
using JobQueue.Core.Models.Enums;
using JobQueue.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

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

    public async Task<Job> GetJobById(Guid jobId)
    {
        var job = await context.Jobs.SingleOrDefaultAsync(j => j.Id == jobId);
        return job ?? throw new ArgumentNullException($"Job with id:{jobId} doesn't exist");
    }

    public async Task<Dictionary<JobStatus, int>> GetJobsCountByAllStatuses()
    {
        return await context.Jobs
            .GroupBy(j => j.Status)
            .Select(g => new { Status = g.Key, TotalCount = g.Count() })
            .ToDictionaryAsync(k => k.Status, v => v.TotalCount);
    }

    public async Task<IEnumerable<Job>> GetFailedJobsPaginated(int page, int pageSize)
    {
        return await context.Jobs.Where(j => j.Status == JobStatus.Failed)
            .Skip(pageSize * (page - 1))
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<DeadLetterJob>> GetDeadLetterQueueJobsPaginated(int page, int pageSize)
    {
        return await context.DeadLetterJobs.Include(d => d.Job)
            .Skip(pageSize * (page - 1))
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task AddJobToDeadLetterQueue(Job job, string reason)
    {
        var deadLetterJob = new DeadLetterJob
        {
            Job = job,
            Reason = reason
        };

        await context.AddAsync(deadLetterJob);
    }

    public async Task RemoveJobFromDeadLetterQueue(Guid jobId)
    {
        var deadLetterJob = await context.DeadLetterJobs.SingleOrDefaultAsync(j => j.JobId == jobId);
        if (deadLetterJob is null)
            throw new ArgumentNullException($"Job with id {jobId} not found in a dead letter queue");

        context.DeadLetterJobs.Remove(deadLetterJob);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}