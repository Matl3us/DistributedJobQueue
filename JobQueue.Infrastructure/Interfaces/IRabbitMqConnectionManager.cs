using RabbitMQ.Client;

namespace JobQueue.Infrastructure.Interfaces;

public interface IRabbitMqConnectionManager
{
    Task InitializeAsync();
    Task<IChannel> CreateChannelAsync();
}