using JobQueue.Infrastructure.Database;
using JobQueue.Infrastructure.Extensions;
using JobQueue.Worker.Configuration;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();

builder.Services.Configure<WorkerOptions>(
    builder.Configuration.GetSection(WorkerOptions.Worker));

builder.Services.AddDbContext<JobContext>(options
    => options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddRabbitMqInfrastructure(builder.Configuration);

//builder.Services.AddHostedService<JobProcessor>();
//builder.Services.AddHostedService<StuckJobsDetector>();
//builder.Services.AddHostedService<DroppedJobsDetector>();

var host = builder.Build();
host.Run();