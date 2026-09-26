using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class DeliveryItemEndpoints
{
    public static IEndpointRouteBuilder MapDeliveryItemEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/delivery-items")
            .WithTags("Delivery Items");

        group.MapPost("select-for-delivery-note", async (DeliveryItemRepository repository, Dictionary<string, JsonElement> body) =>
        {
            string? material = GetOptionalString(body, "Material");
            DateTime deliveryDateFrom = GetRequiredDate(body, "CustomerDoDeliveryDateFrom");
            DateTime deliveryDateTo = GetRequiredDate(body, "CustomerDoDeliveryDateTo");

            if (deliveryDateFrom.Date > deliveryDateTo.Date)
            {
                throw new ArgumentException("Field 'CustomerDoDeliveryDateFrom' must be less than or equal to 'CustomerDoDeliveryDateTo'.");
            }

            return await ApiResponseHelper.HandleAsync(() =>
                repository.SelectForDeliveryNoteAsync(material, deliveryDateFrom, deliveryDateTo));
        });

        return app;
    }

    private static string? GetOptionalString(IReadOnlyDictionary<string, JsonElement> body, string name)
    {
        foreach (KeyValuePair<string, JsonElement> item in body)
        {
            if (!string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (item.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                return null;
            }

            string value = item.Value.ValueKind == JsonValueKind.String
                ? item.Value.GetString() ?? string.Empty
                : item.Value.ToString();

            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        return null;
    }

    private static DateTime GetRequiredDate(IReadOnlyDictionary<string, JsonElement> body, string name)
    {
        foreach (KeyValuePair<string, JsonElement> item in body)
        {
            if (!string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string value = item.Value.ValueKind == JsonValueKind.String
                ? item.Value.GetString() ?? string.Empty
                : item.Value.ToString();

            if (DateTime.TryParse(value, out DateTime date))
            {
                return date.Date;
            }

            break;
        }

        throw new ArgumentException($"Field '{name}' must be a date.");
    }
}
