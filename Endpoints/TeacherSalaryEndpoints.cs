using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class TeacherSalaryEndpoints
{
    public static IEndpointRouteBuilder MapTeacherSalaryEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/teacher-salaries")
            .WithTags("Teacher Salaries");

        group.MapGet("", async (TeacherSalaryRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (TeacherSalaryRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapPost("", async (TeacherSalaryRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("Id", body),
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.Int("TeacherId", body),
                    SqlParameterHelper.Int("SalaryMonth", body),
                    SqlParameterHelper.Int("SalaryYear", body),
                    SqlParameterHelper.Int("TotalSessions", body),
                    SqlParameterHelper.Decimal("BaseSalary", body),
                    SqlParameterHelper.Decimal("BonusAmount", body),
                    SqlParameterHelper.Decimal("DeductionAmount", body),
                    SqlParameterHelper.Decimal("FinalSalary", body),
                    SqlParameterHelper.String("Status", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (TeacherSalaryRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("Id", body),
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.Int("TeacherId", body),
                    SqlParameterHelper.Int("SalaryMonth", body),
                    SqlParameterHelper.Int("SalaryYear", body),
                    SqlParameterHelper.Int("TotalSessions", body),
                    SqlParameterHelper.Decimal("BaseSalary", body),
                    SqlParameterHelper.Decimal("BonusAmount", body),
                    SqlParameterHelper.Decimal("DeductionAmount", body),
                    SqlParameterHelper.Decimal("FinalSalary", body),
                    SqlParameterHelper.String("Status", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{id:int}", async (TeacherSalaryRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });

        return app;
    }
}