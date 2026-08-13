using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class InvoiceDetailEndpoints
{
    public static IEndpointRouteBuilder MapInvoiceDetailEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/invoice-details")
            .WithTags("Invoice Details");

        group.MapGet("", async (InvoiceDetailRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (InvoiceDetailRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapPost("", async (InvoiceDetailRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.NullableInt("Id", body, -1),
                    SqlParameterHelper.NullableInt("UserId", body, 0),
                    SqlParameterHelper.Int("InvoiceId", body),
                    SqlParameterHelper.Int("MaterialId", body),
                    SqlParameterHelper.Int("UnitId", body),
                    SqlParameterHelper.Decimal("Quantity", body),
                    SqlParameterHelper.Decimal("UnitPrice", body),
                    SqlParameterHelper.NullableDecimal("TaxAmount", body),
                    SqlParameterHelper.NullableDecimal("DiscountAmount", body),
                    SqlParameterHelper.Decimal("TotalAmount", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (InvoiceDetailRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.NullableInt("Id", body, -1),
                    SqlParameterHelper.NullableInt("UserId", body, 0),
                    SqlParameterHelper.Int("InvoiceId", body),
                    SqlParameterHelper.Int("MaterialId", body),
                    SqlParameterHelper.Int("UnitId", body),
                    SqlParameterHelper.Decimal("Quantity", body),
                    SqlParameterHelper.Decimal("UnitPrice", body),
                    SqlParameterHelper.NullableDecimal("TaxAmount", body),
                    SqlParameterHelper.NullableDecimal("DiscountAmount", body),
                    SqlParameterHelper.Decimal("TotalAmount", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{id:int}", async (InvoiceDetailRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });

        return app;
    }
}
