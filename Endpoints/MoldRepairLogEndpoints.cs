using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class MoldRepairLogEndpoints
{
    public static IEndpointRouteBuilder MapMoldRepairLogEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/mold-repair-logs")
            .WithTags("Mold Repair Logs");

        group.MapGet("", async (MoldRepairLogRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetAllAsync(-1));
        });

        group.MapGet("{id:int}", async (MoldRepairLogRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetAllAsync(id));
        });

        group.MapPost("", async (MoldRepairLogRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{id:int}", async (MoldRepairLogRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });

        return app;
    }

    private static SqlParameter[] BuildUpsertParameters(Dictionary<string, JsonElement> body)
    {
        return
        [
            SqlParameterHelper.NullableInt("Id", body, -1),
            SqlParameterHelper.Int("MoldId", body),
            SqlParameterHelper.NullableString("Issue", body),
            SqlParameterHelper.NullableString("Solution", body),
            SqlParameterHelper.NullableString("Vendor", body),
            SqlParameterHelper.NullableString("RepairPersonIds", body),
            SqlParameterHelper.NullableDateTime("StartDate", body),
            SqlParameterHelper.NullableDateTime("EndDate", body),
            SqlParameterHelper.NullableString("Result", body),
            SqlParameterHelper.NullableString("Status", body),
            SqlParameterHelper.NullableInt("CreatedBy", body),
            SqlParameterHelper.NullableInt("UpdatedBy", body)
        ];
    }
}
