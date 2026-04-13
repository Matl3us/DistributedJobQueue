using Cronos;
using JobQueue.Core.Interfaces;
using JobQueue.Core.Interfaces.Repositories;
using JobQueue.Core.Models;
using JobQueue.Core.Models.DTOs;
using JobQueue.Core.Models.DTOs.Requests;
using JobQueue.Core.Models.DTOs.Responses;
using JobQueue.Core.Models.Enums;
using System.ComponentModel;
using System.Text.Json;

namespace JobQueue.Application.Services;

public class JobManagementService(IJobRepository jobRepository,
    IRecurringJobRepository recurringJobRepository,
    IOutboxRepository outboxRepository,
    IDeadLetterRepository deadLetterRepository,
    IUnitOfWork unitOfWork)
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
        var job = jobRepository.Add(jobDto);

        var message = new JobMessage()
        {
            Id = job.Id,
            Type = job.Type,
            Priority = job.Priority,
        };
        var payload = JsonSerializer.Serialize(message);
        outboxRepository.Add(job.Id, payload);

        await unitOfWork.CommitAsync();

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

    public async Task<RecurringJobResponse> CreateRecurringJob(CreateRecurringJobRequest request)
    {
        if (!Enum.TryParse<JobType>(request.Type, out var type))
            throw new InvalidEnumArgumentException("Invalid value for job type enum");

        if (!CronExpression.TryParse(request.CronExpression, out var cronExpr))
            throw new CronFormatException("Invalid value for recurring job cron expression");

        var nextRun = cronExpr.GetNextOccurrence(DateTime.UtcNow)
                      ?? throw new InvalidOperationException("Cron expression has no future occurrences");

        var recurringJobDto = new RecurringJobCreate
        {
            Name = request.Name,
            Type = type,
            Payload = request.Payload,
            CronExpression = request.CronExpression,
            NextRun = nextRun
        };
        var recurringJob = recurringJobRepository.Add(recurringJobDto);
        await unitOfWork.CommitAsync();

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

    public async Task<JobResponse> GetJobById(Guid jobId)
    {
        var job = await jobRepository.GetById(jobId);
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

    public async Task<JobsStatusCountResponse> GetJobsCountByAllStatuses()
    {
        var dict = await jobRepository.GetCountByStatus();
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
        var jobs = await jobRepository.GetFailedPaginated(page, pageSize);
        return jobs.Select(j => new FailedJobResponse
        {
            Id = j.Id,
            Type = j.Type.ToString(),
            Priority = j.Priority.ToString(),
            CreatedAt = j.CreatedAt,
            UpdatedAt = j.UpdatedAt,
            Errors = j.Errors
        });
    }

    public async Task<IEnumerable<FailedJobResponse>> GetDeadLetterQueueJobsPaginated(int page, int pageSize)
    {
        var jobs = await deadLetterRepository.GetPaginated(page, pageSize);
        return jobs.Select(j => new FailedJobResponse
        {
            Id = j.Job.Id,
            Type = j.Job.Type.ToString(),
            Priority = j.Job.Priority.ToString(),
            CreatedAt = j.Job.CreatedAt,
            UpdatedAt = j.Job.UpdatedAt,
        });
    }

    public async Task<IEnumerable<RecurringJobResponse>> GetRecurringJobsPaginated(int page, int pageSize)
    {
        var recurringJobs = await recurringJobRepository.GetPaginated(page, pageSize);
        return recurringJobs.Select(r => new RecurringJobResponse
        {
            Id = r.Id,
            Name = r.Name,
            Type = r.Type.ToString(),
            Payload = r.Payload,
            CronExpression = r.CronExpression,
            LastRun = r.LastRun,
            NextRun = r.NextRun
        });
    }

    public async Task DeleteRecurringJob(Guid recurringJobId)
    {
        var recurringJob = await recurringJobRepository.GetById(recurringJobId);
        recurringJobRepository.Remove(recurringJob);
        await unitOfWork.CommitAsync();
    }

    public async Task<bool> RetryJob(Guid jobId)
    {
        try
        {
            var deadLetterJob = await deadLetterRepository.GetByJobId(jobId);
            var job = deadLetterJob.Job;
            deadLetterRepository.Remove(deadLetterJob);
            job.Status = JobStatus.Pending;
            job.RetryCount = 0;

            var message = new JobMessage()
            {
                Id = job.Id,
                Type = job.Type,
                Priority = job.Priority,
            };
            var payload = JsonSerializer.Serialize(message);
            outboxRepository.Add(job.Id, payload);

            await unitOfWork.CommitAsync();

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}