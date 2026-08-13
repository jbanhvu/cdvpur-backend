using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class NewsCategoryEndpoints
{
    public static IEndpointRouteBuilder MapNewsCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/news-categories")
            .WithTags("News Categories");

        group.MapGet("{id:int}", async (NewsCategoryRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.SelectAsync(id));
        });

        group.MapPost("", async (NewsCategoryRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        return app;
    }

    private static SqlParameter[] BuildUpsertParameters(IReadOnlyDictionary<string, JsonElement> body)
    {
        return
        [
            SqlParameterHelper.NullableInt("Id", body, -1),
            SqlParameterHelper.NullableInt("UserId", body, 0),
            SqlParameterHelper.String("Name", body),
            SqlParameterHelper.NullableString("Slug", body),
            SqlParameterHelper.NullableString("Description", body),
            SqlParameterHelper.NullableInt("DisplayOrder", body, 0),
            Bool("IsActive", body, true),
            SqlParameterHelper.NullableDateTime("CreatedDate", body),
            SqlParameterHelper.NullableString("CreatedBy", body)
        ];
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
