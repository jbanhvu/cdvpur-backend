using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class BusinessTripEndpoints
{
    public static IEndpointRouteBuilder MapBusinessTripEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/business-trips")
            .WithTags("Business Trips");

        group.MapGet("", async (BusinessTripRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (BusinessTripRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        return app;
    }
}
