using RabbitMQ.Client;

namespace JobQueue.Infrastructure.Interfaces;

public interface IJobConsumer
{
    Task ConsumeAsync(ReadOnlyMemory<byte> message, ulong deliveryTag, IChannel channel, CancellationToken ct);
}