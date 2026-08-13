using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class RecycleInDetailRepository : BaseRepository
{
    public RecycleInDetailRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_RecycleInDetail_Select",
            new SqlParameter("@Id", 0));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_RecycleInDetail_Select",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> GetByRecycleInIdAsync(int recycleInId)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_RecycleInDetail_SelectByRecycleInId",
            new SqlParameter("@RecycleInId", recycleInId));
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_RecycleInDetail_Upsert",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> BulkSaveAsync(int recycleInId, int userId, string detailsJson)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_RecycleInDetail_BulkSave",
            new SqlParameter("@RecycleInId", recycleInId),
            new SqlParameter("@UserId", userId),
            new SqlParameter("@DetailsJson", detailsJson));
    }

    public async Task<List<Dictionary<string, object?>>> DeleteAsync(int id, int userId)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_RecycleInDetail_Delete",
            new SqlParameter("@Id", id),
            new SqlParameter("@UserId", userId));
    }
}
