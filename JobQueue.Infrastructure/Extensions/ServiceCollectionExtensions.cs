using JobQueue.Core.Interfaces;
using JobQueue.Core.Interfaces.Repositories;
using JobQueue.Infrastructure.Configuration;
using JobQueue.Infrastructure.Interfaces;
using JobQueue.Infrastructure.Messaging;
using JobQueue.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobQueue.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddRabbitMqInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
        services.AddSingleton<IRabbitMqConnectionManager, RabbitMqConnectionManager>();
        services.AddSingleton<IJobPublisher, JobPublisher>();
    }

    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IDeadLetterRepository, DeadLetterRepository>();
        services.AddScoped<IRecurringJobRepository, RecurringJobRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
    }
}