using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class PaymentRepository : BaseRepository
{
    public PaymentRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Payment_Select",
            new SqlParameter("@Id", 0));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Payment_Select",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> GetByEnrollmentIdAsync(int enrollmentId)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Payment_SelectByEnrollment",
            new SqlParameter("@EnrollmentId", enrollmentId));
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Payment_Upsert",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> DeleteAsync(int id, int userId)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Payment_Delete",
            new SqlParameter("@Id", id),
            new SqlParameter("@UserId", userId));
    }
}
