using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class FeedbackRepository : BaseRepository
{
    public FeedbackRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> SelectAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Feedback_Select",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Feedback_Upsert",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> UpdateStatusAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Feedback_UpdateStatus",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> InsertCommentAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_FeedbackComment_Insert",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> SelectCommentsByFeedbackIdAsync(int feedbackId)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_FeedbackComment_SelectByFeedbackId",
            new SqlParameter("@FeedbackId", feedbackId));
    }
}
