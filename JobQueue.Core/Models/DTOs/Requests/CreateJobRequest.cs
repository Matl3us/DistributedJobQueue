using JobQueue.Core.Models.Enums;

namespace JobQueue.Core.Models.DTOs.Requests;

public class CreateJobRequest
{
    public JobType Type { get; init; }
    public string? Payload { get; init; }
}