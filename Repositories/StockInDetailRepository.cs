using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class StockInDetailRepository : BaseRepository
{
    public StockInDetailRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_StockInDetail_Select",
            new SqlParameter("@Id", 0));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_StockInDetail_Select",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> GetByStockInIdAsync(int stockInId)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_StockInDetail_SelectByStockInId",
            new SqlParameter("@StockInId", stockInId));
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_StockInDetail_Upsert",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> BulkSaveAsync(int stockInId, int userId, string detailsJson)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_StockInDetail_BulkSave",
            new SqlParameter("@StockInId", stockInId),
            new SqlParameter("@UserId", userId),
            new SqlParameter("@DetailsJson", detailsJson));
    }

    public async Task<List<Dictionary<string, object?>>> DeleteAsync(int id, int userId)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_StockInDetail_Delete",
            new SqlParameter("@Id", id),
            new SqlParameter("@UserId", userId));
    }
}
