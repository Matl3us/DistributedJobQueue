using JobQueue.Core.Interfaces;
using JobQueue.Core.Models.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace JobQueue.Api.Endpoints;

public static class RecurringJobEndpoints
{
    public static IEndpointRouteBuilder MapRecurringJobEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPost("/",
            async ([FromBody] CreateRecurringJobRequest request,
                IJobManagementService jobService) =>
            {
                var recurringJob = await jobService.CreateRecurringJob(request);
                return Results.Created($"/api/recurring-jobs/{recurringJob.Id}", recurringJob);
            });

        routeBuilder.MapGet("/all",
            async (IJobManagementService jobService,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10) =>
            {
                var recurringJobs = await jobService.GetRecurringJobsPaginated(page, pageSize);
                return Results.Ok(recurringJobs);
            });

        return routeBuilder;
    }
}