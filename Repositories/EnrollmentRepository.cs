using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class EnrollmentRepository : BaseRepository
{
    public EnrollmentRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Enrollment_Select",
            new SqlParameter("@Id", 0));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Enrollment_Select",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> GetByStudentIdAsync(int studentId)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Enrollment_SelectByStudentId",
            new SqlParameter("@StudentId", studentId));
    }

    public async Task<List<Dictionary<string, object?>>> GetPrevCourseAsync(int enrollmentId)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Enrollment_SelectPrevCourse",
            new SqlParameter("@EnrollmentId", enrollmentId));
    }

    public async Task<List<Dictionary<string, object?>>> GetHealingAsync()
    {
        return await ExecuteStoredProcedureAsync("Sol_Enrollment_SelectHealing");
    }

    public async Task<List<Dictionary<string, object?>>> GetNearCompletionAsync(int userId)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Enrollment_NearCompletion_Select",
            new SqlParameter("@UserID", userId));
    }

    public async Task<List<Dictionary<string, object?>>> UpdateEnrollmentNoAsync()
    {
        return await ExecuteStoredProcedureAsync("Sol_Enrollment_UpdateEnrollmentNo");
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Enrollment_Upsert",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> SaveAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Enrollment_Save",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> DeleteAsync(int id, int userId)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Enrollment_Delete",
            new SqlParameter("@Id", id),
            new SqlParameter("@UserId", userId));
    }
}
