using JobQueue.Api.ExceptionHandlers;
using JobQueue.Api.Extensions;
using JobQueue.Application.Services;
using JobQueue.Core.Interfaces;
using JobQueue.Infrastructure.Database;
using JobQueue.Infrastructure.Extensions;
using JobQueue.Infrastructure.Interfaces;
using JobQueue.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;

var allowDashboardFetching = "AllowDashboardFetching";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<JobContext>(options
    => options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddRabbitMqInfrastructure(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddScoped<IJobManagementService, JobManagementService>();
builder.Services.AddHostedService<OutboxProcessor>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(allowDashboardFetching,
        policy => { policy.WithOrigins("http://localhost:5173"); });
});

var app = builder.Build();

var connectionManager = app.Services.GetRequiredService<IRabbitMqConnectionManager>();
await connectionManager.InitializeAsync();

app.UseExceptionHandler();

app.UseCors(allowDashboardFetching);

app.MapGroup("/api")
    .MapEndpoints();

app.Run();