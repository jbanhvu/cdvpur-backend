using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class UserRepository : BaseRepository
{
    public UserRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_User_Select",
            new SqlParameter("@Id", 0));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_User_Select",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> GetByDepartmentCodeAsync(string departmentCode)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_User_SelectByDepartmentCode",
            new SqlParameter("@DepartmentCode", departmentCode));
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_User_Upsert",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> UpdateAvatarAsync(int id, string avatarUrl, int? updatedBy)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_User_UpdateAvatar",
            new SqlParameter("@Id", id),
            new SqlParameter("@AvatarUrl", avatarUrl),
            new SqlParameter("@UpdatedBy", (object?)updatedBy ?? DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> LoginAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_User_Login",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> ChangePasswordAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Cdv_User_ChangePassword",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> DeleteAsync(int id, int userId)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_User_Delete",
            new SqlParameter("@Id", id),
            new SqlParameter("@UserId", userId));
    }
}
