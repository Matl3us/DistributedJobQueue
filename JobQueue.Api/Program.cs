using JobQueue.Api.Extensions;
using JobQueue.Core.Interfaces;
using JobQueue.Infrastructure.Database;
using JobQueue.Infrastructure.Repositories;
using JobQueue.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<JobContext>(options
    => options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobManagementService, JobManagementService>();

var app = builder.Build();

app.MapGet("/", () => "Hello world!");
app.MapGroup("/api/jobs")
    .MapEndpoints();

app.Run();