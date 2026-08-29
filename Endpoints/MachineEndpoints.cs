using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class MachineEndpoints
{
    public static IEndpointRouteBuilder MapMachineEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/machines")
            .WithTags("Machines");

        group.MapGet("", async (MachineRepository repository, string? keyword, bool? isActive) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetAllAsync(keyword, isActive));
        });

        group.MapGet("{machineId:int}", async (MachineRepository repository, int machineId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(machineId));
        });

        group.MapPost("", async (MachineRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{machineId:int}", async (MachineRepository repository, int machineId, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("MachineId", body, machineId);
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{machineId:int}", async (MachineRepository repository, int machineId, string? userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(machineId, userId));
        });

        return app;
    }

    private static SqlParameter[] BuildUpsertParameters(Dictionary<string, JsonElement> body)
    {
        return
        [
            SqlParameterHelper.NullableInt("MachineId", body),
            SqlParameterHelper.String("MachineCode", body),
            SqlParameterHelper.String("MachineName", body),
            SqlParameterHelper.NullableString("MachineGroup", body),
            SqlParameterHelper.NullableBool("IsActive", body),
            SqlParameterHelper.NullableString("Note", body),
            SqlParameterHelper.NullableString("UserId", body)
        ];
    }
}
