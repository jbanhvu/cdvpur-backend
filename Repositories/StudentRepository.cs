using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class StudentRepository : BaseRepository
{
    public StudentRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Student_Select",
            new SqlParameter("@Id", 0));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Student_Select",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> GetOldCourseInfoAsync()
    {
        return await ExecuteStoredProcedureAsync("Sol_Student_SelectOldCourseInfo");
    }

    public async Task<List<Dictionary<string, object?>>> UpdateOldCourseInfoAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Student_UpdateOldCourseInfo",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> UpdateAvatarAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Student_UpdateAvatar",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Student_Upsert",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> DeleteAsync(int id, int userId)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Student_Delete",
            new SqlParameter("@Id", id),
            new SqlParameter("@UserId", userId));
    }
    public async Task<List<Dictionary<string, object?>>> LoginAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Student_Login",
            parameters);
    }
}
