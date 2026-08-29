using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class MachineOperationRepository : BaseRepository
{
    public MachineOperationRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync(
        DateTime dateFrom,
        DateTime dateTo,
        int? machineId,
        string? statusCode,
        int? moldId)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.CDV_MachineOperation_Select",
            new SqlParameter("@OperationLogId", DBNull.Value),
            new SqlParameter("@DateFrom", dateFrom.Date),
            new SqlParameter("@DateTo", dateTo.Date),
            new SqlParameter("@MachineId", (object?)machineId ?? DBNull.Value),
            new SqlParameter("@StatusCode", (object?)statusCode ?? DBNull.Value),
            new SqlParameter("@MoldId", (object?)moldId ?? DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(long operationLogId)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.CDV_MachineOperation_Select",
            new SqlParameter("@OperationLogId", operationLogId),
            new SqlParameter("@DateFrom", DBNull.Value),
            new SqlParameter("@DateTo", DBNull.Value),
            new SqlParameter("@MachineId", DBNull.Value),
            new SqlParameter("@StatusCode", DBNull.Value),
            new SqlParameter("@MoldId", DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.CDV_MachineOperation_Upsert",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> DeleteAsync(long operationLogId)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.CDV_MachineOperation_Delete",
            new SqlParameter("@OperationLogId", operationLogId));
    }

    public async Task<List<Dictionary<string, object?>>> GetMatrixAsync(
        DateTime dateFrom,
        DateTime dateTo,
        int minuteStep,
        int? machineId)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.CDV_MachineOperation_Matrix",
            new SqlParameter("@DateFrom", dateFrom.Date),
            new SqlParameter("@DateTo", dateTo.Date),
            new SqlParameter("@MinuteStep", minuteStep),
            new SqlParameter("@MachineId", (object?)machineId ?? DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> GetSummaryAsync(
        DateTime dateFrom,
        DateTime dateTo,
        int? machineId)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.CDV_MachineOperation_Summary",
            new SqlParameter("@DateFrom", dateFrom.Date),
            new SqlParameter("@DateTo", dateTo.Date),
            new SqlParameter("@MachineId", (object?)machineId ?? DBNull.Value));
    }
}
