using JobQueue.Infrastructure.Database;
using JobQueue.Worker.Configuration;
using JobQueue.Worker.Workers;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<WorkerOptions>(
    builder.Configuration.GetSection(WorkerOptions.Worker));

builder.Services.AddDbContext<JobContext>(options
    => options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddHostedService<JobProcessor>();

var host = builder.Build();
host.Run();