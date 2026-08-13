using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class InvoiceEndpoints
{
    public static IEndpointRouteBuilder MapInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/invoices")
            .WithTags("Invoices");

        group.MapGet("", async (InvoiceRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (InvoiceRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapPost("", async (InvoiceRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.NullableInt("Id", body, -1),
                    SqlParameterHelper.NullableInt("UserId", body, 0),
                    SqlParameterHelper.String("InvoiceNo", body),
                    SqlParameterHelper.Int("SupplierId", body),
                    SqlParameterHelper.Date("InvoiceDate", body),
                    SqlParameterHelper.Decimal("TotalAmount", body),
                    SqlParameterHelper.NullableDecimal("TaxAmount", body),
                    SqlParameterHelper.NullableDecimal("DiscountAmount", body),
                    SqlParameterHelper.Decimal("FinalAmount", body),
                    SqlParameterHelper.NullableString("Status", body),
                    SqlParameterHelper.NullableString("Note", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (InvoiceRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.NullableInt("Id", body, -1),
                    SqlParameterHelper.NullableInt("UserId", body, 0),
                    SqlParameterHelper.String("InvoiceNo", body),
                    SqlParameterHelper.Int("SupplierId", body),
                    SqlParameterHelper.Date("InvoiceDate", body),
                    SqlParameterHelper.Decimal("TotalAmount", body),
                    SqlParameterHelper.NullableDecimal("TaxAmount", body),
                    SqlParameterHelper.NullableDecimal("DiscountAmount", body),
                    SqlParameterHelper.Decimal("FinalAmount", body),
                    SqlParameterHelper.NullableString("Status", body),
                    SqlParameterHelper.NullableString("Note", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{id:int}", async (InvoiceRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });

        return app;
    }
}
