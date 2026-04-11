using JobQueue.Core.Interfaces;
using JobQueue.Core.Models;
using JobQueue.Core.Models.Enums;
using JobQueue.Infrastructure.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace JobQueue.Infrastructure.Messaging;

public class JobPublisher(IRabbitMqConnectionManager rabbitMqConnectionManager) : IJobPublisher
{
    public async Task PublishAsync(JobMessage message, CancellationToken ct = default)
    {
        await using var channel = await rabbitMqConnectionManager.CreateChannelAsync();
        var args = new Dictionary<string, object?>
        {
            { "x-max-priority", 3 }
        };
        var queue = await channel.QueueDeclareAsync(queue: "jobs", durable: true, exclusive: false,
            autoDelete: false, cancellationToken: ct, arguments: args);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        var properties = new BasicProperties
        {
            Priority = MapPriority(message.Priority),
            Persistent = true
        };
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queue.QueueName,
           mandatory: true, body: body, basicProperties: properties, cancellationToken: ct);
    }

    private static byte MapPriority(JobPriority priority)
       => priority switch
       {
           JobPriority.Low => 1,
           JobPriority.Normal => 2,
           JobPriority.High => 3,
           _ => throw new NotImplementedException(),
       };
}