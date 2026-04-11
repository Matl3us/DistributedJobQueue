using JobQueue.Infrastructure.Database;
using JobQueue.Infrastructure.Extensions;
using JobQueue.Infrastructure.Interfaces;
using JobQueue.Worker.Configuration;
using JobQueue.Worker.Extensions;
using JobQueue.Worker.Services;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();

builder.Services.Configure<WorkerOptions>(
    builder.Configuration.GetSection(WorkerOptions.Worker));

builder.Services.AddDbContext<JobContext>(options
    => options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddRabbitMqInfrastructure(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddBackgroundServices();
builder.Services.AddJobHandlers();
builder.Services.AddScoped<IJobConsumer, JobConsumer>();

var host = builder.Build();

var connectionManager = host.Services.GetRequiredService<IRabbitMqConnectionManager>();
await connectionManager.InitializeAsync();

host.Run();