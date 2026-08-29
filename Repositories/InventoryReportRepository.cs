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

    public async Task<List<Dictionary<string, object?>>> GetStockInDetailReportAsync(
        DateTime fromDate,
        DateTime toDate,
        int? materialId,
        int? manufacturerid,
        int? supplierId)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_StockInDetail_Report",
            new SqlParameter("@FromDate", fromDate.Date),
            new SqlParameter("@ToDate", toDate.Date),
            new SqlParameter("@MaterialId", materialId ?? (object)DBNull.Value),
            new SqlParameter("@Manufacturerid", manufacturerid ?? (object)DBNull.Value),
            new SqlParameter("@SupplierId", supplierId ?? (object)DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> GetStockOutDetailReportAsync(
        DateTime fromDate,
        DateTime toDate,
        int? materialId,
        int? manufacturerid)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_StockOutDetail_Report",
            new SqlParameter("@FromDate", fromDate.Date),
            new SqlParameter("@ToDate", toDate.Date),
            new SqlParameter("@MaterialId", materialId ?? (object)DBNull.Value),
            new SqlParameter("@Manufacturerid", manufacturerid ?? (object)DBNull.Value));
    }
}
