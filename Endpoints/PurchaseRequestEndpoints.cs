using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class PurchaseRequestEndpoints
{
    public static IEndpointRouteBuilder MapPurchaseRequestEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/purchase-requests")
            .WithTags("Purchase Requests");

        group.MapGet("", async (PurchaseRequestRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (PurchaseRequestRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapGet("by-department-name", async (PurchaseRequestRepository repository, string departmentName) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByDepartmentNameAsync(departmentName));
        });

        group.MapPost("by-department-name", async (PurchaseRequestRepository repository, Dictionary<string, JsonElement> body) =>
        {
            string? departmentName = GetOptionalString(body, "DepartmentName");

            return await ApiResponseHelper.HandleAsync(() => repository.GetByDepartmentNameAsync(departmentName));
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
}
