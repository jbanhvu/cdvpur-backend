using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class DeliveryNoteEndpoints
{
    public static IEndpointRouteBuilder MapDeliveryNoteEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/delivery-notes")
            .WithTags("Delivery Notes");

        group.MapGet("", async (DeliveryNoteRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (DeliveryNoteRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapPost("", async (DeliveryNoteRepository repository, Dictionary<string, JsonElement> body) =>
        {
            NormalizeBody(body);
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{id:int}", async (DeliveryNoteRepository repository, int id, int userId) =>
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
            SqlParameterHelper.String("DeliveryNo", body),
            SqlParameterHelper.Date("DeliveryDate", body),
            SqlParameterHelper.Int("CustomerId", body),
            SqlParameterHelper.NullableInt("VehicleId", body),
            SqlParameterHelper.NullableString("ReceiverName", body),
            SqlParameterHelper.NullableInt("TotalQuantity", body),
            SqlParameterHelper.NullableString("Remark", body),
            SqlParameterHelper.NullableInt("CreatedBy", body),
            SqlParameterHelper.NullableInt("UpdatedBy", body)
        ];
    }

    private static void NormalizeBody(Dictionary<string, JsonElement> body)
    {
        CopyAlias(body, "DeliveryNoteNo", "DeliveryNo");
        CopyAlias(body, "Note", "Remark");

        if (TryGetValue(body, "UserId", out JsonElement userId))
        {
            string target = IsUpdate(body) ? "UpdatedBy" : "CreatedBy";
            if (!TryGetValue(body, target, out _))
            {
                body[target] = userId;
            }
        }
    }

    private static void CopyAlias(Dictionary<string, JsonElement> body, string sourceName, string targetName)
    {
        if (TryGetValue(body, targetName, out _))
        {
            return;
        }

        if (TryGetValue(body, sourceName, out JsonElement sourceValue))
        {
            body[targetName] = sourceValue;
        }
    }

    private static bool IsUpdate(IReadOnlyDictionary<string, JsonElement> body)
    {
        if (!TryGetValue(body, "Id", out JsonElement idValue))
        {
            return false;
        }

        if (idValue.ValueKind == JsonValueKind.Number && idValue.TryGetInt32(out int id))
        {
            return id > 0;
        }

        return idValue.ValueKind == JsonValueKind.String &&
            int.TryParse(idValue.GetString(), out id) &&
            id > 0;
    }

    private static bool TryGetValue(IReadOnlyDictionary<string, JsonElement> body, string name, out JsonElement value)
    {
        foreach (KeyValuePair<string, JsonElement> item in body)
        {
            if (string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                value = item.Value;
                return true;
            }
        }

        value = default;
        return false;
    }
}
