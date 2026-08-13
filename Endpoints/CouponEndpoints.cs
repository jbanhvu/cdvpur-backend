using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class CouponEndpoints
{
    public static IEndpointRouteBuilder MapCouponEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/coupons")
            .WithTags("Coupons");

        group.MapGet("", async (CouponRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (CouponRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapPost("", async (CouponRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("Id", body),
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.String("Code", body),
                    SqlParameterHelper.String("Name", body),
                    SqlParameterHelper.String("DiscountType", body),
                    SqlParameterHelper.Decimal("DiscountValue", body),
                    SqlParameterHelper.Decimal("MinOrderAmount", body),
                    SqlParameterHelper.Date("StartDate", body),
                    SqlParameterHelper.Date("EndDate", body),
                    SqlParameterHelper.Int("UsageLimit", body),
                    SqlParameterHelper.Int("UsedCount", body),
                    SqlParameterHelper.String("Status", body),
                    SqlParameterHelper.String("Description", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (CouponRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("Id", body),
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.String("Code", body),
                    SqlParameterHelper.String("Name", body),
                    SqlParameterHelper.String("DiscountType", body),
                    SqlParameterHelper.Decimal("DiscountValue", body),
                    SqlParameterHelper.Decimal("MinOrderAmount", body),
                    SqlParameterHelper.Date("StartDate", body),
                    SqlParameterHelper.Date("EndDate", body),
                    SqlParameterHelper.Int("UsageLimit", body),
                    SqlParameterHelper.Int("UsedCount", body),
                    SqlParameterHelper.String("Status", body),
                    SqlParameterHelper.String("Description", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{id:int}", async (CouponRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });

        return app;
    }
}