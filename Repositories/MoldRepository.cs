using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class MoldRepository : BaseRepository
{
    public MoldRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync(string? keyword, bool? isActive)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.CDV_Mold_Select",
            new SqlParameter("@MoldId", DBNull.Value),
            new SqlParameter("@Keyword", (object?)keyword ?? DBNull.Value),
            new SqlParameter("@IsActive", (object?)isActive ?? DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int moldId)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.CDV_Mold_Select",
            new SqlParameter("@MoldId", moldId),
            new SqlParameter("@Keyword", DBNull.Value),
            new SqlParameter("@IsActive", DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.CDV_Mold_Upsert",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> DeleteAsync(int moldId, string? userId)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.CDV_Mold_Delete",
            new SqlParameter("@MoldId", moldId),
            new SqlParameter("@UserId", (object?)userId ?? DBNull.Value));
    }
}
