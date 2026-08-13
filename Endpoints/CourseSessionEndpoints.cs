using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class CourseSessionEndpoints
{
    public static IEndpointRouteBuilder MapCourseSessionEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/course-sessions")
            .WithTags("Course Sessions");

        group.MapGet("", async (CourseSessionRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetAsync(-1));
        });

        group.MapGet("{id:int}", async (CourseSessionRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetAsync(id));
        });

        group.MapGet("by-course/{courseId:int}", async (CourseSessionRepository repository, int courseId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByCourseIdAsync(courseId));
        });

        group.MapPost("", async (CourseSessionRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.NullableInt("Id", body, -1),
                SqlParameterHelper.Int("UserId", body),
                SqlParameterHelper.Int("CourseId", body),
                SqlParameterHelper.Int("SessionNo", body),
                SqlParameterHelper.String("LessonTitle", body),
                SqlParameterHelper.NullableString("LessonContent", body),
                OptionalBool("IsActive", body, true),
                OptionalBool("IsDeleted", body, false)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        return app;
    }

    private static SqlParameter OptionalBool(string name, IReadOnlyDictionary<string, JsonElement> body, bool defaultValue)
    {
        foreach (KeyValuePair<string, JsonElement> item in body)
        {
            if (string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                JsonElement value = item.Value;
                if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                {
                    return new SqlParameter($"@{name}", defaultValue);
                }

                return SqlParameterHelper.Bool(item.Key, body);
            }
        }

        return new SqlParameter($"@{name}", defaultValue);
    }
}
