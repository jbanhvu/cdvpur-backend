using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class ExitPermissionRepository : BaseRepository
{
    public ExitPermissionRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_ExitPermission_Select",
            new SqlParameter("@Id", 0));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_ExitPermission_Select",
            new SqlParameter("@Id", id));
    }
}
