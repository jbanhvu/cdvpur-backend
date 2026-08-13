using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class ApprovalWorkflowStepRepository : BaseRepository
{
    public ApprovalWorkflowStepRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> SelectAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_ApprovalWorkflowStep_Select",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_ApprovalWorkflowStep_Upsert",
            parameters);
    }
}
