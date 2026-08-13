using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class StockOutDetailEndpoints
{
    public static IEndpointRouteBuilder MapStockOutDetailEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/stock-out-details")
            .WithTags("Stock Out Details");

        group.MapGet("", async (StockOutDetailRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (StockOutDetailRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapGet("by-stock-out/{stockOutId:int}", async (StockOutDetailRepository repository, int stockOutId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByStockOutIdAsync(stockOutId));
        });

        group.MapPost("", async (StockOutDetailRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (StockOutDetailRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("by-stock-out/{stockOutId:int}/bulk", async (StockOutDetailRepository repository, int stockOutId, Dictionary<string, JsonElement> body) =>
        {
            int userId = GetOptionalInt(body, "UserId", 0);
            JsonElement details = GetRequired(body, "Details");

            if (details.ValueKind != JsonValueKind.Array)
            {
                throw new ArgumentException("Field 'Details' must be an array.");
            }

            string detailsJson = details.GetRawText();

            return await ApiResponseHelper.HandleAsync(() => repository.BulkSaveAsync(stockOutId, userId, detailsJson));
        });

        group.MapDelete("{id:int}", async (StockOutDetailRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });

        return app;
    }

    private static SqlParameter[] BuildUpsertParameters(Dictionary<string, JsonElement> body)
    {
        return
        [
            SqlParameterHelper.NullableInt("Id", body, -1),
            SqlParameterHelper.NullableInt("UserId", body, 0),
            SqlParameterHelper.NullableInt("StockOutId", body),
            SqlParameterHelper.NullableInt("SupplierId", body),
            SqlParameterHelper.NullableInt("MaterialId", body),
            SqlParameterHelper.NullableDecimal("Qty", body),
            SqlParameterHelper.NullableDecimal("UnitPrice", body),
            SqlParameterHelper.NullableString("Note", body)
        ];
    }

    private static JsonElement GetRequired(IReadOnlyDictionary<string, JsonElement> body, string name)
    {
        foreach (KeyValuePair<string, JsonElement> item in body)
        {
            if (string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                return item.Value;
            }
        }

        throw new ArgumentException($"Missing required field '{name}'.");
    }

    private static int GetOptionalInt(IReadOnlyDictionary<string, JsonElement> body, string name, int defaultValue)
    {
        foreach (KeyValuePair<string, JsonElement> item in body)
        {
            if (!string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (item.Value.ValueKind == JsonValueKind.Number && item.Value.TryGetInt32(out int value))
            {
                return value;
            }

            if (item.Value.ValueKind == JsonValueKind.String && int.TryParse(item.Value.GetString(), out value))
            {
                return value;
            }
        }

        return defaultValue;
    }
}
