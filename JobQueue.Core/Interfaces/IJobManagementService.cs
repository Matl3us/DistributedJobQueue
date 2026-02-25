using JobQueue.Core.Models.DTOs.Requests;
using JobQueue.Core.Models.DTOs.Responses;

namespace JobQueue.Core.Interfaces;

public interface IJobManagementService
{
    Task<JobResponse> CreateJob(CreateJobRequest request);
    Task<JobsStatusCountResponse> GetJobsCountByAllStatuses();
}