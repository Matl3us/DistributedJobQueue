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

        return routeBuilder;
    }
}