using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class StudentEndpoints
{
    public static IEndpointRouteBuilder MapStudentEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/students")
            .WithTags("Students");

        group.MapGet("", async (StudentRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (StudentRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapGet("old-course-info", async (StudentRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetOldCourseInfoAsync);
        });

        group.MapPut("old-course-info/{id:int}", async (StudentRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("ID", body, id);
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Int("ID", body),
                SqlParameterHelper.NullableInt("OldCourseInfoStatusID", body),
                SqlParameterHelper.NullableString("OldCourseInfoNote", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpdateOldCourseInfoAsync(parameters));
        });

        group.MapPost("update-avatar", async (StudentRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Int("Id", body),
                SqlParameterHelper.String("AvatarUrl", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpdateAvatarAsync(parameters));
        });

        group.MapPost("", async (StudentRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });
        group.MapDelete("{id:int}", async (StudentRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });
        group.MapPost("login", async (StudentRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.String("LoginName", body),
                    SqlParameterHelper.String("PasswordHash", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.LoginAsync(parameters));
        });
        return app;
    }

    private static SqlParameter[] BuildUpsertParameters(IReadOnlyDictionary<string, JsonElement> body)
    {
        return
        [
            SqlParameterHelper.Int("Id", body),
            SqlParameterHelper.Int("BranchId", body),
            SqlParameterHelper.String("StudentCode", body),
            SqlParameterHelper.String("FullName", body),
            SqlParameterHelper.String("Gender", body),
            SqlParameterHelper.Date("BirthDate", body),
            SqlParameterHelper.String("Phone", body),
            SqlParameterHelper.String("Email", body),
            SqlParameterHelper.String("Address", body),
            SqlParameterHelper.String("ParentName", body),
            SqlParameterHelper.String("ParentPhone", body),
            SqlParameterHelper.Int("CurrentLevelId", body),
            SqlParameterHelper.Date("JoinDate", body),
            SqlParameterHelper.Bool("IsActive", body),
            SqlParameterHelper.DateTime("CreatedDate", body),
            SqlParameterHelper.String("CreatedBy", body),
            SqlParameterHelper.DateTime("UpdatedDate", body),
            SqlParameterHelper.String("UpdatedBy", body),
            SqlParameterHelper.String("AvatarUrl", body),
            SqlParameterHelper.String("UserName", body),
            SqlParameterHelper.String("PasswordHash", body),
            SqlParameterHelper.NullableInt("EmployeeNo", body, 0)
        ];
    }
}
