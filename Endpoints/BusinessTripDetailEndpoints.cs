using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class BusinessTripDetailEndpoints
{
    public static IEndpointRouteBuilder MapBusinessTripDetailEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/business-trip-details")
            .WithTags("Business Trip Details");

        group.MapGet("", async (BusinessTripDetailRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (BusinessTripDetailRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapGet("by-business-trip/{businessTripId:int}", async (BusinessTripDetailRepository repository, int businessTripId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByBusinessTripIdAsync(businessTripId));
        });

        return app;
    }
}
