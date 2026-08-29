using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class PurchaseDetailEndpoints
{
    public static IEndpointRouteBuilder MapPurchaseDetailEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/purchase-details")
            .WithTags("Purchase Details");

        group.MapGet("", async (PurchaseDetailRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (PurchaseDetailRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapGet("by-purchase/{purchaseId:int}", async (PurchaseDetailRepository repository, int purchaseId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByPurchaseIdAsync(purchaseId));
        });

        return app;
    }
}
