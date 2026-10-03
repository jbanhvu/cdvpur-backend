using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class VehicleEndpoints
{
    public static IEndpointRouteBuilder MapVehicleEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/vehicles")
            .WithTags("Vehicles");

        group.MapGet("", async (VehicleRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (VehicleRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapPost("", async (VehicleRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (VehicleRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPost("bulk", async (VehicleRepository repository, Dictionary<string, JsonElement> body) =>
        {
            int userId = GetOptionalInt(body, "UserId", 0);
            JsonElement vehicles = GetRequired(body, "Vehicles");

            if (vehicles.ValueKind != JsonValueKind.Array)
            {
                throw new ArgumentException("Field 'Vehicles' must be an array.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.BulkUpsertAsync(userId, vehicles.GetRawText()));
        });

        group.MapDelete("{id:int}", async (VehicleRepository repository, int id, int userId) =>
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
            SqlParameterHelper.String("VehicleNo", body),
            SqlParameterHelper.NullableString("DefaultDriverName", body),
            SqlParameterHelper.NullableString("DefaultDriverPhone", body),
            SqlParameterHelper.NullableDecimal("LoadCapacity", body),
            SqlParameterHelper.NullableString("TagRfid", body),
            SqlParameterHelper.NullableBool("IsActive", body)
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
