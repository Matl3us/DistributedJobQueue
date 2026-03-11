using JobQueue.Core.Interfaces;
using JobQueue.Core.Models.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace JobQueue.Api.Endpoints;

public static class JobEndpoints
{
    public static IEndpointRouteBuilder MapJobEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPost("/",
            async ([FromBody] CreateJobRequest request,
                IJobManagementService jobService) =>
            {
                var job = await jobService.CreateJob(request);
                return Results.Created($"/api/jobs/{job.Id}", job);
            });

        routeBuilder.MapGet("/{jobId}",
            async ([FromRoute] Guid jobId,
                IJobManagementService jobService) =>
            {
                var job = await jobService.GetJobById(jobId);
                return Results.Ok(job);
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

        routeBuilder.MapPost("/{jobId}/retry",
            async ([FromRoute] Guid jobId,
                IJobManagementService jobService) =>
            {
                var result = await jobService.RetryJob(jobId);
                return result
                    ? Results.Ok(new { msg = "Job added to pending queue" })
                    : Results.BadRequest(new { error = $"There is no job with id: {jobId} in dead letter queue" });
            });

        return routeBuilder;
    }
}