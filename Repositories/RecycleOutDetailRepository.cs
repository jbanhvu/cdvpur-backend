using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class RecycleOutDetailRepository : BaseRepository
{
    public RecycleOutDetailRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_RecycleOutDetail_Select",
            new SqlParameter("@Id", 0));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_RecycleOutDetail_Select",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> GetByRecycleOutIdAsync(int recycleOutId)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_RecycleOutDetail_SelectByRecycleOutId",
            new SqlParameter("@RecycleOutId", recycleOutId));
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_RecycleOutDetail_Upsert",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> BulkSaveAsync(int recycleOutId, int userId, string detailsJson)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_RecycleOutDetail_BulkSave",
            new SqlParameter("@RecycleOutId", recycleOutId),
            new SqlParameter("@UserId", userId),
            new SqlParameter("@DetailsJson", detailsJson));
    }

    public async Task<List<Dictionary<string, object?>>> DeleteAsync(int id, int userId)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_RecycleOutDetail_Delete",
            new SqlParameter("@Id", id),
            new SqlParameter("@UserId", userId));
    }
}
