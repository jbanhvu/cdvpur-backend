using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class UserFCMTokenRepository : BaseRepository
{
    public UserFCMTokenRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetByUserIdAsync(int userId)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_UserFCMToken_SelectByUserID",
            new SqlParameter("@UserId", userId));
    }

    public async Task<List<Dictionary<string, object?>>> GetByUserAsync(int userId, string? userType)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_UserFCMToken_SelectByUser",
            new SqlParameter("@UserId", userId),
            new SqlParameter("@UserType", string.IsNullOrWhiteSpace(userType) ? DBNull.Value : userType));
    }

    public async Task<List<Dictionary<string, object?>>> GetByRoleAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_UserFCMToken_SelectByRole",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> GetByRoleAsync(int roleId, int branchId)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_UserFCMToken_SelectByRole",
            new SqlParameter("@RoleID", roleId),
            new SqlParameter("@BranchId", branchId));
    }

    public async Task<List<string>> GetTokensByUserIdAsync(int userId)
    {
        List<Dictionary<string, object?>> rows = await GetByUserIdAsync(userId);
        return ExtractTokens(rows);
    }

    public async Task<List<string>> GetTokensByUserAsync(int userId, string? userType)
    {
        List<Dictionary<string, object?>> rows = await GetByUserAsync(userId, userType);
        return ExtractTokens(rows);
    }

    public async Task<List<string>> GetTokensByRoleAsync(int roleId, int branchId)
    {
        List<Dictionary<string, object?>> rows = await GetByRoleAsync(roleId, branchId);
        return ExtractTokens(rows);
    }

    private static List<string> ExtractTokens(List<Dictionary<string, object?>> rows)
    {
        List<string> tokens = new();

        foreach (Dictionary<string, object?> row in rows)
        {
            foreach (KeyValuePair<string, object?> item in row)
            {
                if (!string.Equals(item.Key, "Token", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string? token = item.Value?.ToString();

                if (!string.IsNullOrWhiteSpace(token))
                {
                    tokens.Add(token);
                }
            }
        }

        return tokens;
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_UserFCMToken_Upsert",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> DeleteAsync(string token)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_UserFCMToken_Delete",
            new SqlParameter("@Token", token));
    }

    public async Task DeleteInvalidTokenAsync(string token)
    {
        await DeleteAsync(token);
    }
}
