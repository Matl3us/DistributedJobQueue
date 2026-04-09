namespace JobQueue.Core.Models.Entities;

public class JobError
{
    public Guid Id { get; set; }
    public int AttemptNumber { get; set; }
    public required string ErrorMessage { get; set; }
    public DateTime OccurredAt { get; set; }

    public Guid JobId { get; set; }
    public required Job Job { get; set; }
}