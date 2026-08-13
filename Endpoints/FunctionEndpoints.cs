using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class FunctionEndpoints
{
    public static IEndpointRouteBuilder MapFunctionEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/functions")
            .WithTags("Functions");

        group.MapGet("", async (FunctionRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (FunctionRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapPost("", async (FunctionRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (FunctionRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        return app;
    }

    private static SqlParameter[] BuildUpsertParameters(Dictionary<string, JsonElement> body)
    {
        return
        [
            SqlParameterHelper.NullableInt("Id", body),
            SqlParameterHelper.String("Code", body),
            SqlParameterHelper.NullableInt("ParentId", body),
            SqlParameterHelper.String("Name", body),
            SqlParameterHelper.NullableString("Module", body),
            SqlParameterHelper.NullableString("Route", body),
            SqlParameterHelper.NullableString("Icon", body),
            SqlParameterHelper.NullableInt("SortOrder", body, 0),
            String("FeatureType", body, "MENU"),
            Bool("IsMenu", body, true),
            Bool("IsActive", body, true)
        ];
    }

    private static SqlParameter String(string name, IReadOnlyDictionary<string, JsonElement> body, string defaultValue)
    {
        return TryGetValue(name, body, out JsonElement value) && value.ValueKind != JsonValueKind.Null
            ? SqlParameterHelper.String(name, body)
            : new SqlParameter($"@{name}", defaultValue);
    }

    private static SqlParameter Bool(string name, IReadOnlyDictionary<string, JsonElement> body, bool defaultValue)
    {
        return TryGetValue(name, body, out JsonElement value) && value.ValueKind != JsonValueKind.Null
            ? SqlParameterHelper.Bool(name, body)
            : new SqlParameter($"@{name}", defaultValue);
    }

    private static bool TryGetValue(string name, IReadOnlyDictionary<string, JsonElement> body, out JsonElement value)
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
