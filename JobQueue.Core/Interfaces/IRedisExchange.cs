using StackExchange.Redis;

namespace JobQueue.Core.Interfaces;

public interface IRedisExchange
{
    Task Publish(string queue, string message);
    void Subscribe(string queue, Action<ChannelMessage> callback);
}