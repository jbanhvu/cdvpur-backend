using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using System.Data;
using System.Globalization;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class PurchaseRequestDetailEndpoints
{
    public static IEndpointRouteBuilder MapPurchaseRequestDetailEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/purchase-request-details")
            .WithTags("Purchase Request Details");

        group.MapGet("", async (PurchaseRequestDetailRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (PurchaseRequestDetailRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapGet("by-purchase-request/{purchaseRequestId:int}", async (PurchaseRequestDetailRepository repository, int purchaseRequestId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByPurchaseRequestIdAsync(purchaseRequestId));
        });

        group.MapPut("by-purchase-request/{purchaseRequestId:int}/bulk", async (
            PurchaseRequestDetailRepository repository,
            int purchaseRequestId,
            Dictionary<string, JsonElement> body) =>
        {
            JsonElement details = GetRequired(body, "Details");
            if (details.ValueKind != JsonValueKind.Array)
            {
                throw new ArgumentException("Field 'Details' must be an array.");
            }

            DataTable detailsTable = BuildDetailsTable(details);

            return await ApiResponseHelper.HandleAsync(() => repository.BulkSaveAsync(purchaseRequestId, detailsTable));
        });

        return app;
    }

    private static DataTable BuildDetailsTable(JsonElement details)
    {
        DataTable table = new();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("UseDepartment", typeof(string));
        table.Columns.Add("ItemName", typeof(string));
        table.Columns.Add("Specification", typeof(string));
        table.Columns.Add("Unit", typeof(string));
        table.Columns.Add("Quantity", typeof(decimal));
        table.Columns.Add("EstimatedUnitPrice", typeof(decimal));
        table.Columns.Add("EstimatedAmount", typeof(decimal));

        foreach (JsonElement detail in details.EnumerateArray())
        {
            table.Rows.Add(
                ReadNullableInt(detail, "Id") ?? ReadNullableInt(detail, "id") ?? 0,
                ReadNullableString(detail, "UseDepartment") ?? ReadNullableString(detail, "useDepartment") ?? (object)DBNull.Value,
                ReadRequiredString(detail, "ItemName", "itemName"),
                ReadNullableString(detail, "Specification") ?? ReadNullableString(detail, "specification") ?? (object)DBNull.Value,
                ReadNullableString(detail, "Unit") ?? ReadNullableString(detail, "unit") ?? (object)DBNull.Value,
                ReadRequiredDecimal(detail, "Quantity", "quantity"),
                ReadNullableDecimal(detail, "EstimatedUnitPrice") ?? ReadNullableDecimal(detail, "estimatedUnitPrice") ?? (object)DBNull.Value,
                ReadNullableDecimal(detail, "EstimatedAmount") ?? ReadNullableDecimal(detail, "estimatedAmount") ?? (object)DBNull.Value);
        }

        return table;
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

    private static string ReadRequiredString(JsonElement element, params string[] names)
    {
        foreach (string name in names)
        {
            string? value = ReadNullableString(element, name);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        throw new ArgumentException($"Field '{names[0]}' is required.");
    }

    private static string? ReadNullableString(JsonElement element, string name)
    {
        if (!TryGetProperty(element, name, out JsonElement value) ||
            value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static int? ReadNullableInt(JsonElement element, string name)
    {
        if (!TryGetProperty(element, name, out JsonElement value) ||
            value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int number))
        {
            return number;
        }

        if (value.ValueKind == JsonValueKind.String &&
            int.TryParse(value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
        {
            return number;
        }

        throw new ArgumentException($"Field '{name}' must be an integer.");
    }

    private static decimal ReadRequiredDecimal(JsonElement element, params string[] names)
    {
        foreach (string name in names)
        {
            decimal? value = ReadNullableDecimal(element, name);
            if (value.HasValue)
            {
                return value.Value;
            }
        }

        throw new ArgumentException($"Field '{names[0]}' must be a decimal number.");
    }

    private static decimal? ReadNullableDecimal(JsonElement element, string name)
    {
        if (!TryGetProperty(element, name, out JsonElement value) ||
            value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out decimal number))
        {
            return number;
        }

        if (value.ValueKind == JsonValueKind.String &&
            decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out number))
        {
            return number;
        }

        throw new ArgumentException($"Field '{name}' must be a decimal number.");
    }

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
    {
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }
}
