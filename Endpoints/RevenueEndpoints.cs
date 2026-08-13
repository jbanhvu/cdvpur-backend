using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class RevenueEndpoints
{
    public static IEndpointRouteBuilder MapRevenueEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/revenues")
            .WithTags("Revenues");

        group.MapGet("", async (RevenueRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (RevenueRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapPost("", async (RevenueRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("Id", body),
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.String("RevenueCode", body),
                    SqlParameterHelper.String("RevenueName", body),
                    SqlParameterHelper.String("RevenueType", body),
                    SqlParameterHelper.Date("RevenueDate", body),
                    SqlParameterHelper.Decimal("Amount", body),
                    SqlParameterHelper.String("PaymentMethod", body),
                    SqlParameterHelper.String("ReferenceNo", body),
                    SqlParameterHelper.Int("BranchId", body),
                    SqlParameterHelper.String("Note", body),
                    SqlParameterHelper.String("Status", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (RevenueRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("Id", body),
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.String("RevenueCode", body),
                    SqlParameterHelper.String("RevenueName", body),
                    SqlParameterHelper.String("RevenueType", body),
                    SqlParameterHelper.Date("RevenueDate", body),
                    SqlParameterHelper.Decimal("Amount", body),
                    SqlParameterHelper.String("PaymentMethod", body),
                    SqlParameterHelper.String("ReferenceNo", body),
                    SqlParameterHelper.Int("BranchId", body),
                    SqlParameterHelper.String("Note", body),
                    SqlParameterHelper.String("Status", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{id:int}", async (RevenueRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });

        return app;
    }
}