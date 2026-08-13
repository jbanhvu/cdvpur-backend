using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class DictionaryEndpoints
{
    public static IEndpointRouteBuilder MapDictionaryEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/dictionaries")
            .WithTags("Dictionaries");

        group.MapGet("{id:int}", async (DictionaryRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapPost("by-module", async (DictionaryRepository repository, Dictionary<string, JsonElement> body) =>
        {
            string module = GetRequiredString("Module", body);

            if (string.IsNullOrWhiteSpace(module))
            {
                return ApiResponseHelper.Fail("Module is required.");
            }

            if (module.Length > 100)
            {
                return ApiResponseHelper.Fail("Module must be 100 characters or fewer.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.GetByModuleAsync(module));
        });

        group.MapPost("by-id", async (DictionaryRepository repository, Dictionary<string, JsonElement> body) =>
        {
            int id = GetRequiredInt("ID", body);

            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        return app;
    }

    private static string GetRequiredString(string name, IReadOnlyDictionary<string, JsonElement> body)
    {
        foreach (KeyValuePair<string, JsonElement> item in body)
        {
            if (string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                return item.Value.ValueKind == JsonValueKind.String
                    ? item.Value.GetString() ?? string.Empty
                    : item.Value.ToString();
            }
        }

        return string.Empty;
    }

    private static int GetRequiredInt(string name, IReadOnlyDictionary<string, JsonElement> body)
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

        throw new ArgumentException($"Missing required field '{name}'.");
    }
}
