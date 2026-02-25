using JobQueue.Core.Interfaces;
using JobQueue.Core.Models.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace JobQueue.Api.Extensions;

public static class EndpointRouteBuilderExtension
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPost("/",
            async ([FromBody] CreateJobRequest request,
                IJobManagementService jobService) =>
            {
                var job = await jobService.CreateJob(request);
                return Results.Created($"/api/jobs/{job.Id}", job);
            });

        routeBuilder.MapGet("/status/count",
            async (IJobManagementService jobService) =>
            {
                var statusesCount = await jobService.GetJobsCountByAllStatuses();
                return Results.Ok(statusesCount);
            });

        routeBuilder.MapGet("/failed",
            async (IJobManagementService jobService,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10) =>
            {
                var failedJobs = await jobService.GetFailedJobsPaginated(page, pageSize);
                return Results.Ok(failedJobs);
            });

        routeBuilder.MapGet("/deadLetterQueue",
            async (IJobManagementService jobService,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10) =>
            {
                var deadLetterQueueJobs = await jobService.GetDeadLetterQueueJobsPaginated(page, pageSize);
                return Results.Ok(deadLetterQueueJobs);
            });

        return routeBuilder;
    }
}