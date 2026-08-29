using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class ExitPermissionDetailRepository : BaseRepository
{
    public ExitPermissionDetailRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_ExitPermissionDetail_Select",
            new SqlParameter("@Id", 0),
            new SqlParameter("@ExitPermissionId", DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_ExitPermissionDetail_Select",
            new SqlParameter("@Id", id),
            new SqlParameter("@ExitPermissionId", DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> GetByExitPermissionIdAsync(int exitPermissionId)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_ExitPermissionDetail_Select",
            new SqlParameter("@Id", 0),
            new SqlParameter("@ExitPermissionId", exitPermissionId));
    }
}
