using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class ExpenseEndpoints
{
    public static IEndpointRouteBuilder MapExpenseEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/expenses")
            .WithTags("Expenses");

        group.MapGet("", async (ExpenseRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (ExpenseRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapPost("", async (ExpenseRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("Id", body),
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.String("ExpenseCode", body),
                    SqlParameterHelper.String("ExpenseName", body),
                    SqlParameterHelper.String("ExpenseType", body),
                    SqlParameterHelper.Date("ExpenseDate", body),
                    SqlParameterHelper.Decimal("Amount", body),
                    SqlParameterHelper.String("PaymentMethod", body),
                    SqlParameterHelper.String("ReferenceNo", body),
                    SqlParameterHelper.Int("BranchId", body),
                    SqlParameterHelper.String("Note", body),
                    SqlParameterHelper.String("Status", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (ExpenseRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("Id", body),
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.String("ExpenseCode", body),
                    SqlParameterHelper.String("ExpenseName", body),
                    SqlParameterHelper.String("ExpenseType", body),
                    SqlParameterHelper.Date("ExpenseDate", body),
                    SqlParameterHelper.Decimal("Amount", body),
                    SqlParameterHelper.String("PaymentMethod", body),
                    SqlParameterHelper.String("ReferenceNo", body),
                    SqlParameterHelper.Int("BranchId", body),
                    SqlParameterHelper.String("Note", body),
                    SqlParameterHelper.String("Status", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{id:int}", async (ExpenseRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });

        return app;
    }
}