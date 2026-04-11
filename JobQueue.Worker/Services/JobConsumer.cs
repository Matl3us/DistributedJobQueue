using JobQueue.Core.Interfaces;
using JobQueue.Core.Interfaces.Repositories;
using JobQueue.Core.Models;
using JobQueue.Core.Models.Entities;
using JobQueue.Core.Models.Enums;
using JobQueue.Infrastructure.Interfaces;
using JobQueue.Worker.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace JobQueue.Worker.Services;

public class JobConsumer(IServiceProvider serviceProvider,
    ILogger<JobConsumer> logger, IOptions<WorkerOptions> options) : IJobConsumer
{
    public async Task ConsumeAsync(ReadOnlyMemory<byte> message, ulong deliveryTag, IChannel channel, CancellationToken ct)
    {
        try
        {

            var body = message.ToArray();
            var msg = Encoding.UTF8.GetString(body);
            var jobMsg = JsonSerializer.Deserialize<JobMessage>(msg);
            if (jobMsg is null)
            {
                await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: false, cancellationToken: ct);
                return;
            }

            await using var scope = serviceProvider.CreateAsyncScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            var deadLetterRepository = scope.ServiceProvider.GetRequiredService<IDeadLetterRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var registry = scope.ServiceProvider.GetRequiredService<IHandlerRegistry>();

            var job = await jobRepository.GetById(jobMsg.Id);

            job.Status = JobStatus.Processing;
            job.UpdatedAt = DateTime.UtcNow;
            await unitOfWork.CommitAsync(ct);

            if (string.IsNullOrEmpty(job.Payload))
            {
                await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: false, cancellationToken: ct);
                logger.LogWarning("Empty payload for job with id: {Id}", jobMsg.Id);
                return;
            }
            var result = await registry.HandleAsync(job.Type, job.Payload, ct);

            if (!result.IsSuccess)
            {
                var jobError = new JobError()
                {
                    AttemptNumber = job.RetryCount + 1,
                    ErrorMessage = result.ErrorMessage!,
                    OccurredAt = DateTime.UtcNow,
                    JobId = job.Id
                };

                jobRepository.AddError(jobError);

                job.RetryCount++;
                job.Status = JobStatus.Failed;
                job.UpdatedAt = DateTime.UtcNow;

                var maxRetries = options.Value.MaxJobRetries;
                if (job.RetryCount >= maxRetries)
                {
                    deadLetterRepository.Add(job.Id, $"Max retries ({maxRetries}) reached. Last error: {result.ErrorMessage}");
                }
                else
                {
                    var delay = Math.Pow(2, job.RetryCount);
                    var jitter = Random.Shared.Next(0, 5);
                    job.NextRetryAt = DateTime.UtcNow.AddSeconds(delay + jitter);
                }

                await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: false, ct);
                await unitOfWork.CommitAsync(ct);
                return;
            }

            job.Status = JobStatus.Completed;
            job.Result = result.Payload;
            job.UpdatedAt = DateTime.UtcNow;
            await unitOfWork.CommitAsync(ct);

            await channel.BasicAckAsync(deliveryTag, multiple: false, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error consuming message with delivery tag: {DeliveryTag}", deliveryTag);
            await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: false, ct);
        }
    }
}