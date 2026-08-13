using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class TeacherEndpoints
{
    public static IEndpointRouteBuilder MapTeacherEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/teachers")
            .WithTags("Teachers");

        group.MapGet("", async (TeacherRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (TeacherRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapPost("", async (TeacherRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.NullableInt("Id", body, -1),
                    SqlParameterHelper.NullableInt("UserId", body, 0),
                    SqlParameterHelper.NullableInt("BranchId", body),
                    SqlParameterHelper.NullableString("FullName", body),
                    SqlParameterHelper.NullableString("Phone", body),
                    SqlParameterHelper.NullableString("Email", body),
                    SqlParameterHelper.NullableString("Address", body),
                    SqlParameterHelper.NullableString("Specialty", body),
                    SqlParameterHelper.NullableString("SalaryType", body),
                    SqlParameterHelper.NullableDecimal("SalaryAmount", body),
                    SqlParameterHelper.NullableDate("HireDate", body),
                    SqlParameterHelper.NullableBool("IsActive", body),
                    SqlParameterHelper.NullableInt("TeacherUserID", body),
                    SqlParameterHelper.NullableInt("EmployeeNo", body),
                    SqlParameterHelper.NullableString("Position", body),
                    SqlParameterHelper.NullableString("Description", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{id:int}", async (TeacherRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });

        return app;
    }
}
