using JobQueue.Core.Interfaces;
using JobQueue.Infrastructure.Database;
using JobQueue.Infrastructure.Services;
using JobQueue.Worker.Configuration;
using JobQueue.Worker.Workers;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<WorkerOptions>(
    builder.Configuration.GetSection(WorkerOptions.Worker));

builder.Services.AddDbContext<JobContext>(options
    => options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));
builder.Services.AddSingleton<IJobRedisQueueManagement, JobRedisQueueManagement>();

builder.Services.AddHostedService<JobProcessor>();
builder.Services.AddHostedService<StuckJobsDetector>();
builder.Services.AddHostedService<DroppedJobsDetector>();

var host = builder.Build();
host.Run();