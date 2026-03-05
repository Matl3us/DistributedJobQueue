using Cronos;
using JobQueue.Core.Interfaces;
using JobQueue.Core.Models.DTOs;
using JobQueue.Core.Models.Entities;
using JobQueue.Core.Models.Enums;
using JobQueue.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace JobQueue.Infrastructure.Repositories;

public class JobRepository(JobContext context) : IJobRepository
{
    public async Task<Job> CreateJob(JobCreate jobCreate)
    {
        var job = new Job
        {
            Type = jobCreate.Type,
            Status = JobStatus.Pending,
            Priority = jobCreate.Priority,
            Payload = jobCreate.Payload,
            RecurringJobId = jobCreate.RecurringJobId
        };

        var result = await context.AddAsync(job);
        return result.Entity;
    }

    public async Task<RecurringJob> CreateRecurringJob(RecurringJobCreate recurringJobCreate)
    {
        var recurringJob = new RecurringJob
        {
            Name = recurringJobCreate.Name,
            Type = recurringJobCreate.Type,
            Payload = recurringJobCreate.Payload,
            CronExpression = recurringJobCreate.CronExpression,
            NextRun = recurringJobCreate.NextRun
        };

        var result = await context.AddAsync(recurringJob);
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

    public async Task<IEnumerable<RecurringJob>> GetRecurringJobsPaginated(int page, int pageSize)
    {
        return await context.RecurringJobs.Skip(pageSize * (page - 1))
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Guid?> ScheduleRecurringJob()
    {
        try
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            var scheduledJob = await GetNextScheduledJob();
            if (scheduledJob is null)
                return null;

            var jobDto = new JobCreate
            {
                Type = scheduledJob.Type,
                Priority = JobPriority.High,
                Payload = scheduledJob.Payload,
                RecurringJobId = scheduledJob.Id
            };
            var job = await CreateJob(jobDto);

            await CalculateNextRunForJob(scheduledJob.Id);
            await context.SaveChangesAsync();

            await transaction.CommitAsync();
            return job.Id;
        }
        catch (Exception)
        {
            return null;
        }
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

    public async Task<RecurringJob?> GetRecurringJobById(Guid recurringJobId)
    {
        return await context.RecurringJobs.FirstOrDefaultAsync(r => r.Id == recurringJobId);
    }

    private async Task<RecurringJob?> GetNextScheduledJob()
    {
        var now = new NpgsqlParameter("now", DateTime.UtcNow);
        return await context.RecurringJobs
            .FromSql(
                $$$"""
                       SELECT * FROM public."RecurringJobs" 
                       WHERE "NextRun" <= {{{now}}} 
                       FOR UPDATE SKIP LOCKED LIMIT 1
                   """)
            .FirstOrDefaultAsync();
    }

    private async Task CalculateNextRunForJob(Guid recurringJobId)
    {
        var recurringJob = await context.RecurringJobs.FirstOrDefaultAsync(r => r.Id == recurringJobId);
        if (recurringJob is null)
            throw new ArgumentNullException($"Recurring job with id:{recurringJobId} doesn't exist");


        var nextRun = CronExpression
            .Parse(recurringJob.CronExpression)
            .GetNextOccurrence(DateTime.UtcNow);

        recurringJob.LastRun = recurringJob.NextRun;
        recurringJob.NextRun = nextRun;
    }
}