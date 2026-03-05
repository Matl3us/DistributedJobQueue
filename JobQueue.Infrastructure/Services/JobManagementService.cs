using System.ComponentModel;
using Cronos;
using JobQueue.Core.Interfaces;
using JobQueue.Core.Models.DTOs;
using JobQueue.Core.Models.DTOs.Requests;
using JobQueue.Core.Models.DTOs.Responses;
using JobQueue.Core.Models.Enums;

namespace JobQueue.Infrastructure.Services;

public class JobManagementService(IJobRepository repository, IJobRedisQueueManagement redisQueue)
    : IJobManagementService
{
    public async Task<JobResponse> CreateJob(CreateJobRequest request)
    {
        if (!Enum.TryParse<JobType>(request.Type, out var type))
            throw new InvalidEnumArgumentException("Invalid value for job type enum");

        if (!Enum.TryParse<JobPriority>(request.Priority, out var priority))
            throw new InvalidEnumArgumentException("Invalid value for job priority enum");

        var jobDto = new JobCreate
        {
            Type = type,
            Priority = priority,
            Payload = request.Payload
        };
        var job = await repository.CreateJob(jobDto);
        await repository.SaveChangesAsync();

        await redisQueue.EnqueueAsync(job.Id, job.Priority);

        return new JobResponse
        {
            Id = job.Id,
            Type = job.Type.ToString(),
            Status = job.Status.ToString(),
            Priority = job.Priority.ToString(),
            Payload = job.Payload,
            CreatedAt = job.CreatedAt,
            UpdatedAt = job.UpdatedAt,
            Result = job.Result
        };
    }

    public async Task CreateJobFromRecurring(CreateJobFromRecurringRequest request)
    {
        if (!Enum.TryParse<JobType>(request.Type, out var type))
            throw new InvalidEnumArgumentException("Invalid value for job type enum");

        var jobDto = new JobCreate
        {
            Type = type,
            Priority = JobPriority.High,
            Payload = request.Payload,
            RecurringJobId = request.RecurringJobId
        };
        var job = await repository.CreateJob(jobDto);
        await repository.SaveChangesAsync();

        await redisQueue.EnqueueOnTopAsync(job.Id, job.Priority);
    }

    public async Task<RecurringJobResponse> CreateRecurringJob(CreateRecurringJobRequest request)
    {
        if (!Enum.TryParse<JobType>(request.Type, out var type))
            throw new InvalidEnumArgumentException("Invalid value for job type enum");

        if (!CronExpression.TryParse(request.CronExpression, out var cronExpr))
            throw new CronFormatException("Invalid value for recurring job cron expression");

        var nextRun = cronExpr.GetNextOccurrence(DateTime.UtcNow)
                      ?? throw new CronFormatException("Recurring job doesn't have next occurence");

        var recurringJobDto = new RecurringJobCreate
        {
            Name = request.Name,
            Type = type,
            Payload = request.Payload,
            CronExpression = request.CronExpression,
            NextRun = nextRun
        };
        var recurringJob = await repository.CreateRecurringJob(recurringJobDto);
        await repository.SaveChangesAsync();

        return new RecurringJobResponse
        {
            Id = recurringJob.Id,
            Name = recurringJob.Name,
            Type = recurringJob.Type.ToString(),
            Payload = recurringJob.Payload,
            CronExpression = recurringJob.CronExpression,
            NextRun = recurringJob.NextRun,
            LastRun = recurringJob.LastRun
        };
    }

    public async Task<JobsStatusCountResponse> GetJobsCountByAllStatuses()
    {
        var dict = await repository.GetJobsCountByAllStatuses();
        return new JobsStatusCountResponse
        {
            PendingJobsCount = dict.GetValueOrDefault(JobStatus.Pending, 0),
            ProcessingJobsCount = dict.GetValueOrDefault(JobStatus.Processing, 0),
            CompletedJobsCount = dict.GetValueOrDefault(JobStatus.Completed, 0),
            FailedJobsCount = dict.GetValueOrDefault(JobStatus.Failed, 0)
        };
    }

    public async Task<IEnumerable<FailedJobResponse>> GetFailedJobsPaginated(int page, int pageSize)
    {
        var jobs = await repository.GetFailedJobsPaginated(page, pageSize);
        return jobs.Select(j => new FailedJobResponse
        {
            Id = j.Id,
            Type = j.Type.ToString(),
            Priority = j.Priority.ToString(),
            CreatedAt = j.CreatedAt,
            UpdatedAt = j.UpdatedAt,
            ErrorMessage = j.ErrorMessages
        });
    }

    public async Task<IEnumerable<FailedJobResponse>> GetDeadLetterQueueJobsPaginated(int page, int pageSize)
    {
        var jobs = await repository.GetDeadLetterQueueJobsPaginated(page, pageSize);
        return jobs.Select(j => new FailedJobResponse
        {
            Id = j.Id,
            Type = j.Job.Type.ToString(),
            Priority = j.Job.Priority.ToString(),
            CreatedAt = j.Job.CreatedAt,
            UpdatedAt = j.Job.UpdatedAt,
            ErrorMessage = j.Job.ErrorMessages
        });
    }

    public async Task<RecurringJobResponse?> GetNextScheduledJob()
    {
        var scheduledJob = await repository.GetNextScheduledJob();
        return scheduledJob is null
            ? null
            : new RecurringJobResponse
            {
                Id = scheduledJob.Id,
                Name = scheduledJob.Name,
                Type = scheduledJob.Type.ToString(),
                Payload = scheduledJob.Payload,
                CronExpression = scheduledJob.CronExpression,
                NextRun = scheduledJob.NextRun,
                LastRun = scheduledJob.LastRun
            };
    }

    public async Task CalculateNextRunForJob(Guid recurringJobId)
    {
        await repository.CalculateNextRunForJob(recurringJobId);
        await repository.SaveChangesAsync();
    }

    public async Task<bool> RetryJob(Guid jobId)
    {
        try
        {
            await repository.RemoveJobFromDeadLetterQueue(jobId);
            var job = await repository.GetJobById(jobId);
            job.Status = JobStatus.Pending;
            await redisQueue.EnqueueAsync(job.Id, job.Priority);
            await repository.SaveChangesAsync();
            return true;
        }
        catch (ArgumentNullException)
        {
            return false;
        }
    }
}