using JobQueue.Core.Models.DTOs.Requests;
using JobQueue.Core.Models.DTOs.Responses;

namespace JobQueue.Core.Interfaces;

public interface IJobManagementService
{
    Task<JobResponse> CreateJob(CreateJobRequest request);
    Task<JobsStatusCountResponse> GetJobsCountByAllStatuses();
    Task<IEnumerable<FailedJobResponse>> GetFailedJobsPaginated(int page, int pageSize);
    Task<IEnumerable<FailedJobResponse>> GetDeadLetterQueueJobsPaginated(int page, int pageSize);
    Task<bool> RetryJob(Guid jobId);
}