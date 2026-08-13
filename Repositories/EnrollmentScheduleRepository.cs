using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class EnrollmentScheduleRepository : BaseRepository
{
    public EnrollmentScheduleRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetByEnrollmentIdAsync(int enrollmentId)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_EnrollmentSchedule_SelectByEnrollment",
            new SqlParameter("@EnrollmentId", enrollmentId));
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_EnrollmentSchedule_Upsert",
            parameters);
    }
}
