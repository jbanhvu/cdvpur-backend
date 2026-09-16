using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class InventoryPeriodRepository : BaseRepository
{
    public InventoryPeriodRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> CloseAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "nhvpa3en_vpa01.CDV_sp_InventoryPeriod_Close",
            parameters);
    }
}
