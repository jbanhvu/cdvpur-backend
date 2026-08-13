using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class RecycleOutDetailEndpoints
{
    public static IEndpointRouteBuilder MapRecycleOutDetailEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/recycle-out-details")
            .WithTags("Recycle Out Details");

        group.MapGet("", async (RecycleOutDetailRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (RecycleOutDetailRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapGet("by-recycle-out/{recycleOutId:int}", async (RecycleOutDetailRepository repository, int recycleOutId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByRecycleOutIdAsync(recycleOutId));
        });

        group.MapPost("", async (RecycleOutDetailRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (RecycleOutDetailRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("by-recycle-out/{recycleOutId:int}/bulk", async (RecycleOutDetailRepository repository, int recycleOutId, Dictionary<string, JsonElement> body) =>
        {
            int userId = GetOptionalInt(body, "UserId", 0);
            JsonElement details = GetRequired(body, "Details");

            if (details.ValueKind != JsonValueKind.Array)
            {
                throw new ArgumentException("Field 'Details' must be an array.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.BulkSaveAsync(recycleOutId, userId, details.GetRawText()));
        });

        group.MapDelete("{id:int}", async (RecycleOutDetailRepository repository, int id, int userId) =>
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
            SqlParameterHelper.Int("RecycleOutId", body),
            SqlParameterHelper.Int("MaterialId", body),
            SqlParameterHelper.Decimal("Qty", body),
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
