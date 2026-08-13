using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class SongApprovalRequestRepository : BaseRepository
{
    public SongApprovalRequestRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> SelectAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_SongApprovalRequest_Select",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> SelectByUserIdAsync(int userId)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_SongApprovalRequest_SelectByUserID",
            new SqlParameter("@UserID", userId));
    }

    public async Task<List<Dictionary<string, object?>>> SelectApprovalActionHistoryAsync(int id, int userId, int functionId)
    {
        return await ExecuteStoredProcedureAsync(
            "SongApprovalRequest_Select_ApprovalActionHistory",
            new SqlParameter("@Id", id),
            new SqlParameter("@UserID", userId),
            new SqlParameter("@FunctionID", functionId));
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_SongApprovalRequest_Upsert",
            parameters);
    }
}
