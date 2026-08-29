using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class MachineOperationEndpoints
{
    public static IEndpointRouteBuilder MapMachineOperationEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/machine-operations")
            .WithTags("Machine Operations");

        group.MapGet("", async (
            MachineOperationRepository repository,
            DateTime? dateFrom,
            DateTime? dateTo,
            int? machineId,
            string? statusCode,
            int? moldId) =>
        {
            if (dateFrom is null || dateTo is null)
            {
                return ApiResponseHelper.Fail("Query parameters 'dateFrom' and 'dateTo' are required.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.GetAllAsync(
                dateFrom.Value,
                dateTo.Value,
                machineId,
                statusCode,
                moldId));
        });

        group.MapGet("matrix", async (
            MachineOperationRepository repository,
            DateTime? dateFrom,
            DateTime? dateTo,
            int? minuteStep,
            int? machineId) =>
        {
            if (dateFrom is null || dateTo is null)
            {
                return ApiResponseHelper.Fail("Query parameters 'dateFrom' and 'dateTo' are required.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.GetMatrixAsync(
                dateFrom.Value,
                dateTo.Value,
                minuteStep ?? 60,
                machineId));
        });

        group.MapGet("summary", async (
            MachineOperationRepository repository,
            DateTime? dateFrom,
            DateTime? dateTo,
            int? machineId) =>
        {
            if (dateFrom is null || dateTo is null)
            {
                return ApiResponseHelper.Fail("Query parameters 'dateFrom' and 'dateTo' are required.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.GetSummaryAsync(
                dateFrom.Value,
                dateTo.Value,
                machineId));
        });

        group.MapGet("{operationLogId:long}", async (MachineOperationRepository repository, long operationLogId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(operationLogId));
        });

        group.MapPost("", async (MachineOperationRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{operationLogId:long}", async (MachineOperationRepository repository, long operationLogId, Dictionary<string, JsonElement> body) =>
        {
            SetLong("OperationLogId", body, operationLogId);
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{operationLogId:long}", async (MachineOperationRepository repository, long operationLogId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(operationLogId));
        });

        return app;
    }

    private static SqlParameter[] BuildUpsertParameters(Dictionary<string, JsonElement> body)
    {
        return
        [
            NullableBigInt("OperationLogId", body),
            SqlParameterHelper.Int("MachineId", body),
            SqlParameterHelper.DateTime("StartTime", body),
            SqlParameterHelper.DateTime("EndTime", body),
            SqlParameterHelper.String("StatusCode", body),
            SqlParameterHelper.NullableInt("MoldId", body),
            SqlParameterHelper.NullableString("Note", body),
            SqlParameterHelper.NullableInt("CreatedBy", body),
            SqlParameterHelper.NullableDateTime("CreatedAt", body),
            SqlParameterHelper.NullableInt("UpdatedBy", body),
            SqlParameterHelper.NullableDateTime("UpdatedAt", body)
        ];
    }

    private static SqlParameter NullableBigInt(string name, IReadOnlyDictionary<string, JsonElement> body)
    {
        foreach (KeyValuePair<string, JsonElement> item in body)
        {
            if (!string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (item.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                return new SqlParameter($"@{name}", DBNull.Value);
            }

            if (item.Value.ValueKind == JsonValueKind.Number && item.Value.TryGetInt64(out long number))
            {
                return new SqlParameter($"@{name}", number);
            }

            if (item.Value.ValueKind == JsonValueKind.String &&
                long.TryParse(item.Value.GetString(), out number))
            {
                return new SqlParameter($"@{name}", number);
            }
        }

        return new SqlParameter($"@{name}", DBNull.Value);
    }

    private static void SetLong(string name, Dictionary<string, JsonElement> body, long value)
    {
        string? key = body.Keys.FirstOrDefault(item => string.Equals(item, name, StringComparison.OrdinalIgnoreCase));
        if (key is not null)
        {
            body.Remove(key);
        }

        body[name] = JsonSerializer.SerializeToElement(value);
    }
}
