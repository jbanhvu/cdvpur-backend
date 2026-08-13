using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class EnrollmentScheduleEndpoints
{
    public static IEndpointRouteBuilder MapEnrollmentScheduleEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/enrollment-schedules")
            .WithTags("Enrollment Schedules");

        group.MapGet("by-enrollment/{enrollmentId:int}", async (EnrollmentScheduleRepository repository, int enrollmentId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByEnrollmentIdAsync(enrollmentId));
        });

        group.MapPost("", async (EnrollmentScheduleRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Int("EnrollmentId", body),
                SqlParameterHelper.Int("UserId", body),
                SqlParameterHelper.String("Schedules", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        return app;
    }
}
