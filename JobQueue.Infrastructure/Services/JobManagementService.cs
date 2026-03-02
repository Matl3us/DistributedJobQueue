using System.ComponentModel;
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

        var jobDto = new JobCreateDto
        {
            Type = type,
            Payload = request.Payload
        };
        var job = await repository.CreateJob(jobDto);
        await repository.SaveChangesAsync();

        await redisQueue.EnqueueAsync(job.Id);

        return new JobResponse
        {
            Id = job.Id,
            Type = job.Type.ToString(),
            Status = job.Status.ToString(),
            Payload = job.Payload,
            CreatedAt = job.CreatedAt,
            UpdatedAt = job.UpdatedAt,
            Result = job.Result
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
            CreatedAt = j.Job.CreatedAt,
            UpdatedAt = j.Job.UpdatedAt,
            ErrorMessage = j.Job.ErrorMessages
        });
    }

    public async Task<bool> RetryJob(Guid jobId)
    {
        try
        {
            await repository.RemoveJobFromDeadLetterQueue(jobId);
            var job = await repository.GetJobById(jobId);
            job.Status = JobStatus.Pending;
            await repository.SaveChangesAsync();
            return true;
        }
        catch (ArgumentNullException)
        {
            return false;
        }
    }
}