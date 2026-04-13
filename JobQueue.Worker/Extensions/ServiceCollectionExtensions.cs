using JobQueue.Core.Interfaces;
using JobQueue.Core.Models.DTOs.JobPayloads;
using JobQueue.Infrastructure.Messaging;
using JobQueue.Worker.BackgroundServices;
using JobQueue.Worker.Handlers;
using JobQueue.Worker.Services;

namespace JobQueue.Worker.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<JobScheduler>();
        services.AddHostedService<RetryScheduler>();
        services.AddHostedService<RabbitMqConsumer>();
        services.AddHostedService<OutboxProcessor>();
    }

    public static void AddJobHandlers(this IServiceCollection services)
    {
        services.AddScoped<IJobHandler<SendEmailPayload>, SendEmailHandler>();
        services.AddScoped<IJobHandler<GeneratePdfPayload>, GeneratePdfHandler>();
        services.AddScoped<IJobHandler<ProcessImagePayload>, ProcessImageHandler>();
        services.AddScoped<IJobHandler<DeliverWebhookPayload>, DeliverWebhookHandler>();
        services.AddScoped<IHandlerRegistry, HandlerRegistry>();
    }
}