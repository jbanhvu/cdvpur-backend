using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class PurchaseOrderDetailEndpoints
{
    public static IEndpointRouteBuilder MapPurchaseOrderDetailEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/purchase-order-details")
            .WithTags("Purchase Order Details");

        group.MapGet("", async (PurchaseOrderDetailRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (PurchaseOrderDetailRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapPost("", async (PurchaseOrderDetailRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("Id", body),
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.Int("PurchaseOrderId", body),
                    SqlParameterHelper.Int("ProductId", body),
                    SqlParameterHelper.Int("Quantity", body),
                    SqlParameterHelper.Decimal("UnitPrice", body),
                    SqlParameterHelper.Decimal("TotalAmount", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (PurchaseOrderDetailRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("Id", body),
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.Int("PurchaseOrderId", body),
                    SqlParameterHelper.Int("ProductId", body),
                    SqlParameterHelper.Int("Quantity", body),
                    SqlParameterHelper.Decimal("UnitPrice", body),
                    SqlParameterHelper.Decimal("TotalAmount", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{id:int}", async (PurchaseOrderDetailRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });

        return app;
    }
}