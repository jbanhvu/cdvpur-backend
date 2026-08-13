using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class NewsEndpoints
{
    public static IEndpointRouteBuilder MapNewsEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/news")
            .WithTags("News");

        group.MapPost("select", async (NewsRepository repository, Dictionary<string, JsonElement> body) =>
        {
            int id = GetOptionalInt("Id", body, 0);
            int categoryId = GetOptionalInt("CategoryId", body, 0);

            return await ApiResponseHelper.HandleAsync(() => repository.SelectAsync(id, categoryId));
        });

        group.MapPost("", async (NewsRepository repository, Dictionary<string, JsonElement> body) =>
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
            SqlParameterHelper.Int("CategoryId", body),
            SqlParameterHelper.String("Title", body),
            SqlParameterHelper.NullableString("Slug", body),
            SqlParameterHelper.NullableString("Summary", body),
            SqlParameterHelper.NullableString("Content", body),
            SqlParameterHelper.NullableString("ThumbnailUrl", body),
            SqlParameterHelper.NullableString("CoverUrl", body),
            SqlParameterHelper.NullableString("AuthorName", body),
            SqlParameterHelper.NullableDateTime("PublishedDate", body),
            SqlParameterHelper.NullableInt("ViewCount", body, 0),
            Bool("IsHot", body, false),
            Bool("IsPublished", body, true),
            SqlParameterHelper.NullableString("Tags", body),
            SqlParameterHelper.NullableString("SeoTitle", body),
            SqlParameterHelper.NullableString("SeoDescription", body),
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

    private static int GetOptionalInt(string name, IReadOnlyDictionary<string, JsonElement> body, int defaultValue)
    {
        return TryGetValue(name, body, out JsonElement value) && value.ValueKind != JsonValueKind.Null
            ? (int)SqlParameterHelper.NullableInt(name, body, defaultValue).Value
            : defaultValue;
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
