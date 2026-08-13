using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class StockOutDetailRepository : BaseRepository
{
    public StockOutDetailRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_StockOutDetail_Select",
            new SqlParameter("@Id", 0));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_StockOutDetail_Select",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> GetByStockOutIdAsync(int stockOutId)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_StockOutDetail_SelectByStockOutId",
            new SqlParameter("@StockOutId", stockOutId));
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_StockOutDetail_Upsert",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> BulkSaveAsync(int stockOutId, int userId, string detailsJson)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_StockOutDetail_BulkSave",
            new SqlParameter("@StockOutId", stockOutId),
            new SqlParameter("@UserId", userId),
            new SqlParameter("@DetailsJson", detailsJson));
    }

    public async Task<List<Dictionary<string, object?>>> DeleteAsync(int id, int userId)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_StockOutDetail_Delete",
            new SqlParameter("@Id", id),
            new SqlParameter("@UserId", userId));
    }
}
