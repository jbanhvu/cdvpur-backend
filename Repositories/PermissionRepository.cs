using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class PermissionRepository : BaseRepository
{
    public PermissionRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> SelectAsync(int? id)
    {
        return await ExecuteStoredProcedureAsync(
            "sp_Sol_Permission_Select",
            new SqlParameter("@Id", id ?? (object)DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "sp_Sol_Permission_Upsert",
            parameters);
    }
}
