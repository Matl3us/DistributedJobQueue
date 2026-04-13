using JobQueue.Infrastructure.Interfaces;
using JobQueue.Worker.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace JobQueue.Worker.BackgroundServices;

public class RabbitMqConsumer(IRabbitMqConnectionManager rabbitMqConnectionManager,
    IJobConsumer jobConsumer,
    IOptions<WorkerOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var prefetchCount = options.Value.PrefetchCount;

        try
        {

            await using var channel = await rabbitMqConnectionManager.CreateChannelAsync();
            var args = new Dictionary<string, object?>
            {
                { "x-max-priority", 3 }
            };
            var queue = await channel.QueueDeclareAsync(queue: "jobs", durable: true, exclusive: false,
                autoDelete: false, cancellationToken: stoppingToken, arguments: args);
            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: prefetchCount, global: false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                await jobConsumer.ConsumeAsync(ea.Body, ea.DeliveryTag, channel, stoppingToken);
            };
            await channel.BasicConsumeAsync(queue: queue.QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        { }
    }
}