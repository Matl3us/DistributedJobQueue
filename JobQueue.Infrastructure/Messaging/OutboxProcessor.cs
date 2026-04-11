using JobQueue.Core.Interfaces;
using JobQueue.Core.Interfaces.Repositories;
using JobQueue.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace JobQueue.Infrastructure.Messaging;

public class OutboxProcessor(IServiceProvider serviceProvider,
    IJobPublisher jobPublisher,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var unpublished = await outboxRepository.GetUnpublishedAsync(5);

            if (!unpublished.Any())
            {
                await Task.Delay(3000, stoppingToken);
                continue;
            }

            foreach (var outbox in unpublished)
            {
                try
                {

                    var message = JsonSerializer.Deserialize<JobMessage>(outbox.Payload);
                    if (message is null)
                    {
                        logger.LogError("Outbox with id: {Id} has invalid payload", outbox.Id);
                        continue;
                    }

                    await jobPublisher.PublishAsync(message, stoppingToken);
                    outboxRepository.MarkPublished(outbox);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing outbox");
                    await Task.Delay(3000, stoppingToken);
                }
            }

            await unitOfWork.CommitAsync(stoppingToken);
        }
    }
}