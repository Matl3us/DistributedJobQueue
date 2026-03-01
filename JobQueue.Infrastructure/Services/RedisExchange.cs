using JobQueue.Core.Interfaces;
using StackExchange.Redis;

namespace JobQueue.Infrastructure.Services;

public class RedisExchange(string connectionString) : IRedisExchange
{
    private readonly ConnectionMultiplexer _redis = ConnectionMultiplexer.Connect(connectionString);

    public async Task Publish(string queue, string message)
    {
        var sub = _redis.GetSubscriber();
        await sub.PublishAsync(RedisChannel.Literal(queue), message);
    }

    public void Subscribe(string queue, Action<ChannelMessage> callback)
    {
        var sub = _redis.GetSubscriber();
        sub.Subscribe(RedisChannel.Literal(queue)).OnMessage(callback);
    }
}