namespace JobQueue.Core.Models.Entities;

public class Outbox
{
    public Guid Id { get; set; }
    public required string Payload { get; set; }
    public bool Published { get; set; }
    public DateTime CreatedAt { get; set; }

    public Guid JobId { get; set; }
    public Job Job { get; set; }
}