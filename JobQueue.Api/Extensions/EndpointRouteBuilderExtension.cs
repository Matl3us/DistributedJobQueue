using JobQueue.Api.Endpoints;

namespace JobQueue.Api.Extensions;

public static class EndpointRouteBuilderExtension
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGroup("/jobs")
            .MapJobEndpoints();

        routeBuilder.MapGroup("/recurring-jobs")
            .MapRecurringJobEndpoints();

        return routeBuilder;
    }
}