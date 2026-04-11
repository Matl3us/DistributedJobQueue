using JobQueue.Core.Interfaces.Repositories;
using JobQueue.Core.Models.DTOs;
using JobQueue.Core.Models.Entities;
using JobQueue.Core.Models.Enums;
using JobQueue.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace JobQueue.Infrastructure.Repositories;

public class JobRepository(JobContext context) : IJobRepository
{
    public Job Add(JobCreate jobCreate)
    {

        var job = new Job
        {
            Type = jobCreate.Type,
            Status = JobStatus.Pending,
            Priority = jobCreate.Priority,
            Payload = jobCreate.Payload,
            RecurringJobId = jobCreate.RecurringJobId
        };

        var result = context.Add(job);
        return result.Entity;
    }

    public void AddError(JobError jobError)
    {
        context.JobErrors.Add(jobError);
    }

    public async Task<Job> GetById(Guid jobId)
    {
        var job = await context.Jobs.SingleOrDefaultAsync(j => j.Id == jobId);
        return job ?? throw new ArgumentNullException($"Job with id:{jobId} doesn't exist");
    }

    public async Task<Dictionary<JobStatus, int>> GetCountByStatus()
    {
        return await context.Jobs
            .GroupBy(j => j.Status)
            .Select(g => new { Status = g.Key, TotalCount = g.Count() })
            .ToDictionaryAsync(k => k.Status, v => v.TotalCount);
    }

    public async Task<IEnumerable<Job>> GetFailedPaginated(int page, int pageSize)
    {
        return await context.Jobs.Where(j => j.Status == JobStatus.Failed)
            .Include(j => j.Errors)
            .Skip(pageSize * (page - 1))
            .Take(pageSize)
            .ToListAsync();
    }

    public void UpdateStatus(Job job, JobStatus status, string? result = null)
    {
        job.Status = status;
        if (result != null) job.Result = result;
    }
}