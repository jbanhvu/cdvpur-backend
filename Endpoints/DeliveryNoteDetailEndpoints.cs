using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class DeliveryNoteDetailEndpoints
{
    public static IEndpointRouteBuilder MapDeliveryNoteDetailEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/delivery-note-details")
            .WithTags("Delivery Note Details");

        group.MapGet("", async (DeliveryNoteDetailRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (DeliveryNoteDetailRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapGet("by-delivery-note/{deliveryNoteId:int}", async (DeliveryNoteDetailRepository repository, int deliveryNoteId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByDeliveryNoteIdAsync(deliveryNoteId));
        });

        group.MapPost("", async (DeliveryNoteDetailRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (DeliveryNoteDetailRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("by-delivery-note/{deliveryNoteId:int}/bulk", async (DeliveryNoteDetailRepository repository, int deliveryNoteId, Dictionary<string, JsonElement> body) =>
        {
            int userId = GetOptionalInt(body, "UserId", 0);
            JsonElement details = GetRequired(body, "Details");

            if (details.ValueKind != JsonValueKind.Array)
            {
                throw new ArgumentException("Field 'Details' must be an array.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.BulkSaveAsync(deliveryNoteId, userId, details.GetRawText()));
        });

        group.MapDelete("{id:int}", async (DeliveryNoteDetailRepository repository, int id, int userId) =>
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
            SqlParameterHelper.Int("DeliveryNoteId", body),
            SqlParameterHelper.NullableInt("Seq", body),
            SqlParameterHelper.Int("ProductId", body),
            SqlParameterHelper.Int("Quantity", body),
            SqlParameterHelper.NullableString("DeliveryTime", body),
            SqlParameterHelper.NullableString("TrolleyBox", body),
            SqlParameterHelper.NullableString("DeliveryTag", body),
            SqlParameterHelper.NullableString("RFIDTag", body),
            SqlParameterHelper.NullableString("Remark", body)
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
