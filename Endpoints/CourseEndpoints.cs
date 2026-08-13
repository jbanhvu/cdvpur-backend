using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class CourseEndpoints
{
    public static IEndpointRouteBuilder MapCourseEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/courses")
            .WithTags("Courses");

        group.MapGet("", async (CourseRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (CourseRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapPost("", async (CourseRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("Id", body),
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.String("Name", body),
                    SqlParameterHelper.String("Code", body),
                    SqlParameterHelper.Int("LevelId", body),
                    SqlParameterHelper.Int("DurationMonths", body),
                    SqlParameterHelper.Int("TotalSessions", body),
                    SqlParameterHelper.Decimal("TuitionFee", body),
                    SqlParameterHelper.String("Description", body),
                    SqlParameterHelper.Bool("IsActive", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (CourseRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("Id", body),
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.String("Name", body),
                    SqlParameterHelper.String("Code", body),
                    SqlParameterHelper.Int("LevelId", body),
                    SqlParameterHelper.Int("DurationMonths", body),
                    SqlParameterHelper.Int("TotalSessions", body),
                    SqlParameterHelper.Decimal("TuitionFee", body),
                    SqlParameterHelper.String("Description", body),
                    SqlParameterHelper.Bool("IsActive", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{id:int}", async (CourseRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });

        return app;
    }
}