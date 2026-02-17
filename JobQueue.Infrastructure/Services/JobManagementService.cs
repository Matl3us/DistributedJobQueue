using System.ComponentModel;
using JobQueue.Core.Interfaces;
using JobQueue.Core.Models.DTOs;
using JobQueue.Core.Models.DTOs.Requests;
using JobQueue.Core.Models.DTOs.Responses;
using JobQueue.Core.Models.Enums;

namespace JobQueue.Infrastructure.Services;

public class JobManagementService(IJobRepository repository) : IJobManagementService
{
    public async Task<JobResponse> CreateJob(CreateJobRequest request)
    {
        if (!Enum.TryParse<JobType>(request.Type, out var type))
            throw new InvalidEnumArgumentException();

        var jobDto = new JobCreateDto
        {
            Type = type,
            Payload = request.Payload
        };
        var job = await repository.CreateJob(jobDto);
        await repository.SaveChangesAsync();

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
}