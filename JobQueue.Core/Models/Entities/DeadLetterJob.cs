namespace JobQueue.Core.Models.Entities;

public class DeadLetterJob
{
    public Guid Id { get; set; }
    public required string Reason { get; set; }
    public DateTime CreatedAt { get; set; }

    public Guid JobId { get; set; }
    public Job Job { get; set; }
}