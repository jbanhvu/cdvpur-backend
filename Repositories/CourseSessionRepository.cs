using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class CourseSessionRepository : BaseRepository
{
    public CourseSessionRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_CourseSession_Select",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> GetByCourseIdAsync(int courseId)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_CourseSession_SelectByCourseId",
            new SqlParameter("@CourseId", courseId));
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_CourseSession_Upsert",
            parameters);
    }
}
