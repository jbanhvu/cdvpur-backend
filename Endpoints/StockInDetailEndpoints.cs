using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class StockInDetailEndpoints
{
    public static IEndpointRouteBuilder MapStockInDetailEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/stock-in-details")
            .WithTags("Stock In Details");

        group.MapGet("", async (StockInDetailRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (StockInDetailRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapGet("by-stock-in/{stockInId:int}", async (StockInDetailRepository repository, int stockInId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByStockInIdAsync(stockInId));
        });

        group.MapPost("", async (StockInDetailRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (StockInDetailRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("by-stock-in/{stockInId:int}/bulk", async (StockInDetailRepository repository, int stockInId, Dictionary<string, JsonElement> body) =>
        {
            int userId = GetOptionalInt(body, "UserId", 0);
            JsonElement details = GetRequired(body, "Details");

            if (details.ValueKind != JsonValueKind.Array)
            {
                throw new ArgumentException("Field 'Details' must be an array.");
            }

            string detailsJson = details.GetRawText();

            return await ApiResponseHelper.HandleAsync(() => repository.BulkSaveAsync(stockInId, userId, detailsJson));
        });

        group.MapDelete("{id:int}", async (StockInDetailRepository repository, int id, int userId) =>
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
            SqlParameterHelper.Int("StockInId", body),
            SqlParameterHelper.NullableInt("Manufacturerid", body),
            SqlParameterHelper.Int("MaterialId", body),
            SqlParameterHelper.Decimal("Qty", body),
            SqlParameterHelper.NullableDecimal("UnitPrice", body),
            SqlParameterHelper.NullableString("LotNo", body),
            SqlParameterHelper.NullableDate("ExpiredDate", body),
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
