using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class PurchaseRequestLinkEndpoints
{
    public static IEndpointRouteBuilder MapPurchaseRequestLinkEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/purchase-request-links")
            .WithTags("Purchase Request Links");

        group.MapGet("", async (PurchaseRequestLinkRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (PurchaseRequestLinkRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapGet("by-purchase/{purchaseId:int}", async (PurchaseRequestLinkRepository repository, int purchaseId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByPurchaseIdAsync(purchaseId));
        });

        return app;
    }
}
