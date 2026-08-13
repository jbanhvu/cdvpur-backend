using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class PurchaseOrderEndpoints
{
    public static IEndpointRouteBuilder MapPurchaseOrderEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/purchase-orders")
            .WithTags("Purchase Orders");

        group.MapGet("", async (PurchaseOrderRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (PurchaseOrderRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapPost("", async (PurchaseOrderRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("Id", body),
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.Int("SupplierId", body),
                    SqlParameterHelper.Date("OrderDate", body),
                    SqlParameterHelper.Decimal("TotalAmount", body),
                    SqlParameterHelper.String("Status", body),
                    SqlParameterHelper.String("Note", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (PurchaseOrderRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("Id", body),
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.Int("SupplierId", body),
                    SqlParameterHelper.Date("OrderDate", body),
                    SqlParameterHelper.Decimal("TotalAmount", body),
                    SqlParameterHelper.String("Status", body),
                    SqlParameterHelper.String("Note", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{id:int}", async (PurchaseOrderRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });

        return app;
    }
}