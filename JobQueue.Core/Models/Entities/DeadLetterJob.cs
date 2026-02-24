namespace JobQueue.Core.Models.Entities;

public class DeadLetterJob
{
    public Guid Id { get; set; }
    public required string Reason { get; set; }

    public Guid JobId { get; set; }
    public required Job Job { get; set; }
}