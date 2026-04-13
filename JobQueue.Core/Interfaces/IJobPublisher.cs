using JobQueue.Core.Models;

namespace JobQueue.Core.Interfaces;

public interface IJobPublisher
{
    Task PublishAsync(JobMessage message, CancellationToken ct = default);
}