using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class MachineRepository : BaseRepository
{
    public MachineRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync(string? keyword, bool? isActive)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.CDV_Machine_Select",
            new SqlParameter("@MachineId", DBNull.Value),
            new SqlParameter("@Keyword", (object?)keyword ?? DBNull.Value),
            new SqlParameter("@IsActive", (object?)isActive ?? DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int machineId)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.CDV_Machine_Select",
            new SqlParameter("@MachineId", machineId),
            new SqlParameter("@Keyword", DBNull.Value),
            new SqlParameter("@IsActive", DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.CDV_Machine_Upsert",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> DeleteAsync(int machineId, string? userId)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.CDV_Machine_Delete",
            new SqlParameter("@MachineId", machineId),
            new SqlParameter("@UserId", (object?)userId ?? DBNull.Value));
    }
}
