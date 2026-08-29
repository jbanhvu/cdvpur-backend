using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class MachineStatusRepository : BaseRepository
{
    public MachineStatusRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync(bool? isActive)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.CDV_MachineStatus_Select",
            new SqlParameter("@IsActive", (object?)isActive ?? DBNull.Value));
    }
}
