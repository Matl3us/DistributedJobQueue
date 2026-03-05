using JobQueue.Api.ExceptionHandlers;
using JobQueue.Api.Extensions;
using JobQueue.Core.Interfaces;
using JobQueue.Infrastructure.Database;
using JobQueue.Infrastructure.Repositories;
using JobQueue.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var allowDashboardFetching = "AllowDashboardFetching";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<JobContext>(options
    => options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobManagementService, JobManagementService>();
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));
builder.Services.AddSingleton<IJobRedisQueueManagement, JobRedisQueueManagement>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(allowDashboardFetching,
        policy => { policy.WithOrigins("http://localhost:5173"); });
});

var app = builder.Build();

app.UseExceptionHandler();

app.UseCors(allowDashboardFetching);

app.MapGroup("/api")
    .MapEndpoints();

app.Run();