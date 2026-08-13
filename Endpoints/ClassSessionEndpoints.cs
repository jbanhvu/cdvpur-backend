using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class ClassSessionEndpoints
{
    public static IEndpointRouteBuilder MapClassSessionEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/class-sessions")
            .WithTags("Class Sessions");

        group.MapGet("{id:int}", async (ClassSessionRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapGet("by-class/{classId:int}", async (ClassSessionRepository repository, int classId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByClassIdAsync(classId));
        });

        group.MapPost("by-session-date", async (ClassSessionRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Date("FromDate", body),
                SqlParameterHelper.Date("ToDate", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.GetBySessionDateAsync(parameters));
        });

        group.MapPost("", async (ClassSessionRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("Id", body),
                    SqlParameterHelper.Int("ClassId", body),
                    SqlParameterHelper.Int("TeacherId", body),
                    SqlParameterHelper.Int("SessionNo", body),
                    SqlParameterHelper.Date("SessionDate", body),
                    SqlParameterHelper.Time("StartTime", body),
                    SqlParameterHelper.Time("EndTime", body),
                    SqlParameterHelper.String("Topic", body),
                    SqlParameterHelper.String("Homework", body),
                    SqlParameterHelper.Int("RoomId", body),
                    SqlParameterHelper.String("Status", body),
                    SqlParameterHelper.DateTime("CreatedAt", body),
                    SqlParameterHelper.String("CreatedBy", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{id:int}", async (ClassSessionRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });

        return app;
    }
}
