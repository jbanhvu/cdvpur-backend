using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class EnrollmentEndpoints
{
    public static IEndpointRouteBuilder MapEnrollmentEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/enrollments")
            .WithTags("Enrollments");

        group.MapGet("", async (EnrollmentRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("healing", async (EnrollmentRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetHealingAsync);
        });

        group.MapGet("{id:int}", async (EnrollmentRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapGet("by-student/{studentId:int}", async (EnrollmentRepository repository, int studentId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByStudentIdAsync(studentId));
        });

        group.MapGet("{enrollmentId:int}/prev-course", async (EnrollmentRepository repository, int enrollmentId) =>
        {
            if (enrollmentId <= 0)
            {
                return ApiResponseHelper.Fail("EnrollmentId must be greater than 0.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.GetPrevCourseAsync(enrollmentId));
        });

        group.MapGet("near-completion/{userId:int}", async (EnrollmentRepository repository, int userId) =>
        {
            if (userId <= 0)
            {
                return ApiResponseHelper.Fail("UserId must be greater than 0.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.GetNearCompletionAsync(userId));
        });

        group.MapPost("update-enrollment-no", async (EnrollmentRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.UpdateEnrollmentNoAsync);
        });

        group.MapPost("", async (EnrollmentRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.NullableInt("Id", body, -1),
                    SqlParameterHelper.NullableInt("UserId", body, 0),
                    SqlParameterHelper.Int("StudentId", body),
                    SqlParameterHelper.Int("BranchId", body),
                    SqlParameterHelper.Int("CourseId", body),
                    SqlParameterHelper.NullableInt("RoomId", body),
                    SqlParameterHelper.NullableInt("TeacherId", body),
                    SqlParameterHelper.String("Status", body),
                    SqlParameterHelper.Decimal("TuitionFee", body),
                    SqlParameterHelper.Decimal("DiscountAmount", body),
                    SqlParameterHelper.Decimal("FinalAmount", body),
                    SqlParameterHelper.NullableDate("StartDate", body),
                    SqlParameterHelper.NullableDate("EndDate", body),
                    SqlParameterHelper.String("Note", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPost("save", async (EnrollmentRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.NullableInt("Id", body, -1),
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.Int("StudentId", body),
                    SqlParameterHelper.Int("BranchId", body),
                    SqlParameterHelper.Int("CourseId", body),
                    SqlParameterHelper.NullableInt("RoomId", body),
                    SqlParameterHelper.NullableInt("TeacherId", body),
                    SqlParameterHelper.String("Status", body),
                    SqlParameterHelper.Decimal("TuitionFee", body),
                    SqlParameterHelper.Decimal("DiscountAmount", body),
                    SqlParameterHelper.Decimal("FinalAmount", body),
                    SqlParameterHelper.Date("StartDate", body),
                    SqlParameterHelper.Date("EndDate", body),
                    SqlParameterHelper.String("Schedules", body),
                    SqlParameterHelper.Bool("IsReSchedules", body),
                    SqlParameterHelper.NullableString("Note", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.SaveAsync(parameters));
        });

        group.MapPut("{id:int}", async (EnrollmentRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.NullableInt("Id", body, -1),
                    SqlParameterHelper.NullableInt("UserId", body, 0),
                    SqlParameterHelper.Int("StudentId", body),
                    SqlParameterHelper.Int("BranchId", body),
                    SqlParameterHelper.Int("CourseId", body),
                    SqlParameterHelper.NullableInt("RoomId", body),
                    SqlParameterHelper.NullableInt("TeacherId", body),
                    SqlParameterHelper.String("Status", body),
                    SqlParameterHelper.Decimal("TuitionFee", body),
                    SqlParameterHelper.Decimal("DiscountAmount", body),
                    SqlParameterHelper.Decimal("FinalAmount", body),
                    SqlParameterHelper.NullableDate("StartDate", body),
                    SqlParameterHelper.NullableDate("EndDate", body),
                    SqlParameterHelper.String("Note", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{id:int}", async (EnrollmentRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });

        return app;
    }
}
