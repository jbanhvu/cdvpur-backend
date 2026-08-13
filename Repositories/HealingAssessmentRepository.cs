using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class HealingAssessmentRepository : BaseRepository
{
    public HealingAssessmentRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_HealingAssessment_SelectById",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> GetByEnrollmentIdAsync(int enrollmentId)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_HealingAssessment_SelectByEnrollmentId",
            new SqlParameter("@EnrollmentId", enrollmentId));
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_HealingAssessment_Upsert",
            parameters);
    }
}
