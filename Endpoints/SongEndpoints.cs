using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class SongEndpoints
{
    public static IEndpointRouteBuilder MapSongEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/songs")
            .WithTags("Songs");

        group.MapGet("", async (SongRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.SelectAsync(0));
        });

        group.MapGet("{id:int}", async (SongRepository repository, int id) =>
        {
            if (id < 0)
            {
                return ApiResponseHelper.Fail("Id must be greater than or equal to 0.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.SelectAsync(id));
        });

        group.MapPost("", async (SongRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Int("Id", body),
                SqlParameterHelper.NullableString("Code", body),
                SqlParameterHelper.String("Name", body),
                SqlParameterHelper.NullableString("Composer", body),
                SqlParameterHelper.NullableString("Genre", body),
                SqlParameterHelper.NullableInt("DifficultyLevel", body),
                SqlParameterHelper.NullableString("Description", body),
                SqlParameterHelper.Bool("IsActive", body),
                SqlParameterHelper.Int("SortOrder", body),
                SqlParameterHelper.Int("UserId", body)
            ];

            if ((int)parameters[0].Value < 0)
            {
                return ApiResponseHelper.Fail("Id must be greater than or equal to 0.");
            }

            if (string.IsNullOrWhiteSpace(parameters[2].Value?.ToString()))
            {
                return ApiResponseHelper.Fail("Name is required.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        return app;
    }
}
