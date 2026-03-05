namespace JobQueue.Core.Models.DTOs.Requests;

public class CreateJobFromRecurringRequest
{
    public required string Type { get; init; }
    public string? Payload { get; init; }
    public Guid RecurringJobId { get; init; }
}