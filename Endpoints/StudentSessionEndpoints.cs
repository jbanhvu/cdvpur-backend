using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class StudentSessionEndpoints
{
    public static IEndpointRouteBuilder MapStudentSessionEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/student-sessions")
            .WithTags("Student Sessions");

        group.MapGet("", async (StudentSessionRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (StudentSessionRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapGet("by-enrollment/{enrollmentId:int}", async (StudentSessionRepository repository, int enrollmentId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByEnrollmentIdAsync(enrollmentId));
        });

        group.MapPost("user", async (StudentSessionRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Int("UserId", body),
                SqlParameterHelper.NullableDate("FromDate", body),
                SqlParameterHelper.NullableDate("ToDate", body)
            ];

            if ((int)parameters[0].Value <= 0)
            {
                return ApiResponseHelper.Fail("UserId must be greater than 0.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.GetByUserIdAsync(parameters));
        });

        group.MapPost("by-session-date", async (StudentSessionRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Date("FromDate", body),
                SqlParameterHelper.Date("ToDate", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.GetBySessionDateAsync(parameters));
        });

        group.MapPost("teacher-teaching-schedule", async (StudentSessionRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Int("UserId", body),
                SqlParameterHelper.Date("FromDate", body),
                SqlParameterHelper.Date("ToDate", body)
            ];

            if ((int)parameters[0].Value <= 0)
            {
                return ApiResponseHelper.Fail("UserId must be greater than 0.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.GetTeacherTeachingScheduleAsync(parameters));
        });

        group.MapPost("room-schedule", async (StudentSessionRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Date("FromDate", body),
                SqlParameterHelper.Date("ToDate", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.GetRoomScheduleAsync(parameters));
        });

        group.MapGet("current-class/{userId:int}", async (StudentSessionRepository repository, int userId) =>
        {
            if (userId <= 0)
            {
                return ApiResponseHelper.Fail("UserID must be greater than 0.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.GetCurrentClassAsync(userId));
        });

        group.MapPost("update-attendance", async (StudentSessionRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Int("Id", body),
                SqlParameterHelper.NullableString("AttendanceStatus", body),
                SqlParameterHelper.NullableDateTime("CheckInAt", body),
                SqlParameterHelper.NullableDateTime("CheckOutAt", body)
            ];

            if ((int)parameters[0].Value <= 0)
            {
                return ApiResponseHelper.Fail("Id must be greater than 0.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.UpdateAttendanceAsync(parameters));
        });

        group.MapPost("sync-attendance", async (StudentSessionRepository repository, JsonElement body) =>
        {
            SqlParameter jsonParameter = new("@Json", SqlDbType.NVarChar, -1)
            {
                Value = body.GetRawText()
            };

            return await ApiResponseHelper.HandleAsync(() => repository.SyncAttendanceAsync(jsonParameter));
        });

        group.MapPost("update-makeup-schedule/{enrollmentId:int}", async (StudentSessionRepository repository, int enrollmentId, JsonElement body) =>
        {
            if (enrollmentId <= 0)
            {
                return ApiResponseHelper.Fail("EnrollmentId must be greater than 0.");
            }

            SqlParameter[] parameters =
            [
                new("@EnrollmentId", enrollmentId),
                new("@Json", SqlDbType.NVarChar, -1)
                {
                    Value = body.GetRawText()
                }
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpdateMakeupScheduleAsync(parameters));
        });

        group.MapPost("update-evaluation", async (StudentSessionRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Int("ID", body),
                SqlParameterHelper.NullableDecimal("Score", body),
                SqlParameterHelper.String("TeacherComment", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpdateEvaluationAsync(parameters));
        });

        group.MapPost("update-teacher", async (StudentSessionRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Int("Id", body),
                SqlParameterHelper.Int("TeacherId", body)
            ];

            if ((int)parameters[0].Value <= 0)
            {
                return ApiResponseHelper.Fail("Id must be greater than 0.");
            }

            if ((int)parameters[1].Value <= 0)
            {
                return ApiResponseHelper.Fail("TeacherId must be greater than 0.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.UpdateTeacherAsync(parameters));
        });

        group.MapPost("reschedule", async (StudentSessionRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Int("StudentSessionId", body),
                SqlParameterHelper.Date("NewDate", body),
                SqlParameterHelper.Int("UserId", body)
            ];

            if ((int)parameters[0].Value <= 0)
            {
                return ApiResponseHelper.Fail("StudentSessionId must be greater than 0.");
            }

            if ((int)parameters[2].Value <= 0)
            {
                return ApiResponseHelper.Fail("UserId must be greater than 0.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.RescheduleAsync(parameters));
        });

        group.MapPost("remove-gap", async (StudentSessionRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Int("EnrollmentId", body),
                SqlParameterHelper.Date("RemoveDate", body),
                SqlParameterHelper.Int("UserId", body)
            ];

            if ((int)parameters[0].Value <= 0)
            {
                return ApiResponseHelper.Fail("EnrollmentId must be greater than 0.");
            }

            if ((int)parameters[2].Value <= 0)
            {
                return ApiResponseHelper.Fail("UserId must be greater than 0.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.RemoveGapAsync(parameters));
        });

        group.MapPost("insert-gap", async (StudentSessionRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Int("EnrollmentId", body),
                SqlParameterHelper.Date("InsertDate", body),
                SqlParameterHelper.Time("InsertStartTime", body),
                SqlParameterHelper.Time("InsertEndTime", body),
                SqlParameterHelper.Int("UserId", body)
            ];

            if ((int)parameters[0].Value <= 0)
            {
                return ApiResponseHelper.Fail("EnrollmentId must be greater than 0.");
            }

            if ((int)parameters[4].Value <= 0)
            {
                return ApiResponseHelper.Fail("UserId must be greater than 0.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.InsertGapAsync(parameters));
        });

        group.MapPost("change-gap", async (StudentSessionRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Int("EnrollmentId", body),
                SqlParameterHelper.Date("RemoveDate", body),
                SqlParameterHelper.Date("InsertDate", body),
                SqlParameterHelper.Time("InsertStartTime", body),
                SqlParameterHelper.Time("InsertEndTime", body),
                SqlParameterHelper.Int("UserId", body)
            ];

            if ((int)parameters[0].Value <= 0)
            {
                return ApiResponseHelper.Fail("EnrollmentId must be greater than 0.");
            }

            if ((int)parameters[5].Value <= 0)
            {
                return ApiResponseHelper.Fail("UserId must be greater than 0.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.ChangeGapAsync(parameters));
        });

        group.MapPost("", async (StudentSessionRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (StudentSessionRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{id:int}", async (StudentSessionRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id));
        });

        return app;
    }

    private static SqlParameter[] BuildUpsertParameters(Dictionary<string, JsonElement> body)
    {
        return
        [
            SqlParameterHelper.Int("Id", body),
            SqlParameterHelper.Int("EnrollmentId", body),
            SqlParameterHelper.Int("StudentId", body),
            SqlParameterHelper.Int("OriginalSessionId", body),
            SqlParameterHelper.Int("ActualSessionId", body),
            SqlParameterHelper.Int("TeacherId", body),
            SqlParameterHelper.String("AttendanceStatus", body),
            SqlParameterHelper.Decimal("Score", body),
            SqlParameterHelper.String("PerformanceLevel", body),
            SqlParameterHelper.String("TeacherComment", body),
            SqlParameterHelper.String("Homework", body),
            SqlParameterHelper.Int("PracticeMinutes", body),
            SqlParameterHelper.Date("MakeupDate", body),
            SqlParameterHelper.Time("MakeupStartTime", body),
            SqlParameterHelper.Time("MakeupEndTime", body),
            SqlParameterHelper.Int("MakeupTeacherId", body),
            SqlParameterHelper.String("MakeupLocation", body),
            SqlParameterHelper.String("Note", body),
            SqlParameterHelper.DateTime("CreatedAt", body),
            SqlParameterHelper.String("CreatedBy", body)
        ];
    }
}
