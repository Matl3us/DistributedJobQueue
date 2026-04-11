using JobQueue.Api.ExceptionHandlers;
using JobQueue.Api.Extensions;
using JobQueue.Application.BackgroundServices;
using JobQueue.Infrastructure.Database;
using JobQueue.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

var allowDashboardFetching = "AllowDashboardFetching";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<JobContext>(options
    => options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddRabbitMqInfrastructure(builder.Configuration);
builder.Services.AddRepositories();

builder.Services.AddHostedService<JobScheduler>();
builder.Services.AddHostedService<OutboxProcessor>();

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