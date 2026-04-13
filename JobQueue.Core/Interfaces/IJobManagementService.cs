using JobQueue.Core.Models.DTOs.Requests;
using JobQueue.Core.Models.DTOs.Responses;

namespace JobQueue.Core.Interfaces;

public interface IJobManagementService
{
    Task<JobResponse> CreateJob(CreateJobRequest request);
    Task<RecurringJobResponse> CreateRecurringJob(CreateRecurringJobRequest request);
    Task<JobResponse> GetJobById(Guid jobId);
    Task<JobsStatusCountResponse> GetJobsCountByAllStatuses();
    Task<IEnumerable<FailedJobResponse>> GetFailedJobsPaginated(int page, int pageSize);
    Task<IEnumerable<FailedJobResponse>> GetDeadLetterQueueJobsPaginated(int page, int pageSize);
    Task<IEnumerable<RecurringJobResponse>> GetRecurringJobsPaginated(int page, int pageSize);
    Task DeleteRecurringJob(Guid recurringJobId);
    Task<bool> RetryJob(Guid jobId);
}