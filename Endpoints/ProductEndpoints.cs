using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/products")
            .WithTags("Products");

        group.MapGet("", async (ProductRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (ProductRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapPost("", async (ProductRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("Id", body),
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.Int("CategoryId", body),
                    SqlParameterHelper.String("Name", body),
                    SqlParameterHelper.String("SKU", body),
                    SqlParameterHelper.String("Barcode", body),
                    SqlParameterHelper.Decimal("Price", body),
                    SqlParameterHelper.Decimal("CostPrice", body),
                    SqlParameterHelper.Int("StockQuantity", body),
                    SqlParameterHelper.Int("WarrantyMonths", body),
                    SqlParameterHelper.String("Description", body),
                    SqlParameterHelper.Bool("IsActive", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (ProductRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("Id", body),
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.Int("CategoryId", body),
                    SqlParameterHelper.String("Name", body),
                    SqlParameterHelper.String("SKU", body),
                    SqlParameterHelper.String("Barcode", body),
                    SqlParameterHelper.Decimal("Price", body),
                    SqlParameterHelper.Decimal("CostPrice", body),
                    SqlParameterHelper.Int("StockQuantity", body),
                    SqlParameterHelper.Int("WarrantyMonths", body),
                    SqlParameterHelper.String("Description", body),
                    SqlParameterHelper.Bool("IsActive", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{id:int}", async (ProductRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });

        return app;
    }
}