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

        return routeBuilder;
    }
}