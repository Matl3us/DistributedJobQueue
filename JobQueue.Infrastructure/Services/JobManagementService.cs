using JobQueue.Core.Interfaces;
using JobQueue.Core.Models.DTOs.Requests;
using JobQueue.Core.Models.DTOs.Responses;

namespace JobQueue.Infrastructure.Services;

public class JobManagementService(IJobRepository repository) : IJobManagementService
{
    public async Task<JobDto> CreateJob(CreateJobRequest request)
    {
        var job = await repository.CreateJob(request);
        await repository.SaveChangesAsync();

        return new JobDto
        {
            Id = job.Id,
            Type = job.Type,
            Status = job.Status,
            Payload = job.Payload,
            CreatedAt = job.CreatedAt,
            UpdatedAt = job.UpdatedAt,
            Result = job.Result
        };
    }
}