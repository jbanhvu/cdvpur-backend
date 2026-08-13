using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class InventoryReportRepository : BaseRepository
{
    public InventoryReportRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetInventoryReportAsync(int year, int month)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_Inventory_Report",
            new SqlParameter("@Year", year),
            new SqlParameter("@Month", month));
    }
}
