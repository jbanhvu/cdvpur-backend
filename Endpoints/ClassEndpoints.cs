using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class ClassEndpoints
{
    public static IEndpointRouteBuilder MapClassEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/classes")
            .WithTags("Classes");

        group.MapGet("", async (ClassRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (ClassRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapPost("", async (ClassRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("Id", body),
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.Int("BranchId", body),
                    SqlParameterHelper.Int("CourseId", body),
                    SqlParameterHelper.Int("TeacherId", body),
                    SqlParameterHelper.Int("RoomId", body),
                    SqlParameterHelper.String("Name", body),
                    SqlParameterHelper.String("Code", body),
                    SqlParameterHelper.Date("StartDate", body),
                    SqlParameterHelper.Date("EndDate", body),
                    SqlParameterHelper.Int("MaxStudents", body),
                    SqlParameterHelper.String("DaysOfWeek", body),
                    SqlParameterHelper.Time("StartTime", body),
                    SqlParameterHelper.Time("EndTime", body),
                    SqlParameterHelper.String("Status", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (ClassRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("Id", body),
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.Int("BranchId", body),
                    SqlParameterHelper.Int("CourseId", body),
                    SqlParameterHelper.Int("TeacherId", body),
                    SqlParameterHelper.Int("RoomId", body),
                    SqlParameterHelper.String("Name", body),
                    SqlParameterHelper.String("Code", body),
                    SqlParameterHelper.Date("StartDate", body),
                    SqlParameterHelper.Date("EndDate", body),
                    SqlParameterHelper.Int("MaxStudents", body),
                    SqlParameterHelper.String("DaysOfWeek", body),
                    SqlParameterHelper.Time("StartTime", body),
                    SqlParameterHelper.Time("EndTime", body),
                    SqlParameterHelper.String("Status", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{id:int}", async (ClassRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });

        return app;
    }
}