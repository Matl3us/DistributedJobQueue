using JobQueue.Infrastructure.Database;
using JobQueue.Worker.Workers;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<JobContext>(options
    => options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddHostedService<JobProcessor>();

var host = builder.Build();
host.Run();