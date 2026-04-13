using JobQueue.Core.Interfaces;
using JobQueue.Core.Models;
using JobQueue.Core.Models.DTOs.JobPayloads;
using JobQueue.Core.Models.Enums;
using System.Text.Json;

namespace JobQueue.Worker.Services;

public class HandlerRegistry : IHandlerRegistry
{
    private readonly Dictionary<JobType, Func<string, CancellationToken, Task<JobResult>>> _handlers = [];
    private readonly IServiceProvider _serviceProvider;

    public HandlerRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        AddHandler<SendEmailPayload>(JobType.SendEmail);
        AddHandler<GeneratePdfPayload>(JobType.GeneratePdf);
        AddHandler<ProcessImagePayload>(JobType.ProcessImage);
        AddHandler<DeliverWebhookPayload>(JobType.DeliverWebhook);
    }

    public Task<JobResult> HandleAsync(JobType type, string payload, CancellationToken ct)
    {
        if (!_handlers.TryGetValue(type, out var handler))
            throw new InvalidOperationException($"No handler registered for job type {type}");

        return handler(payload, ct);
    }

    private void AddHandler<T>(JobType type)
    {
        _handlers[type] = (payload, ct) =>
        {
            var typedPayload = JsonSerializer.Deserialize<T>(payload) ?? throw new ArgumentNullException("Handler payload cannot be null");
            var handler = _serviceProvider.GetRequiredService<IJobHandler<T>>();
            return handler.HandleAsync(typedPayload, ct);
        };
    }
}