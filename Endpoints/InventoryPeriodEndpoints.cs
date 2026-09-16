using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class InventoryPeriodEndpoints
{
    public static IEndpointRouteBuilder MapInventoryPeriodEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/inventory-periods")
            .WithTags("Inventory Periods");

        group.MapPost("close", async (InventoryPeriodRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Int("PeriodYear", body),
                SqlParameterHelper.Int("PeriodMonth", body),
                SqlParameterHelper.NullableDateTime("ClosedAt", body),
                SqlParameterHelper.NullableInt("ClosedBy", body),
                SqlParameterHelper.NullableDateTime("ReclosedAt", body),
                SqlParameterHelper.NullableInt("ReclosedBy", body),
                SqlParameterHelper.NullableString("Remark", body),
                SqlParameterHelper.NullableInt("CreatedBy", body),
                SqlParameterHelper.NullableInt("UpdatedBy", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.CloseAsync(parameters));
        });

        return app;
    }
}
