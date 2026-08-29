using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class PurchaseEndpoints
{
    public static IEndpointRouteBuilder MapPurchaseEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/purchases")
            .WithTags("Purchases");

        group.MapGet("", async (PurchaseRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (PurchaseRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapGet("pu02-by-pu01", async (PurchaseRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetPu02ByPu01Async);
        });

        return app;
    }
}
