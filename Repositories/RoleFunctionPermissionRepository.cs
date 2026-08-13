using ChangdaeVinaPurchasingApi.Base;
using ChangdaeVinaPurchasingApi.Dtos;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class RoleFunctionPermissionRepository : BaseRepository
{
    public RoleFunctionPermissionRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_RoleFunctionPermission_Select",
            new SqlParameter("@Id", DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_RoleFunctionPermission_Select",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> GetByUserIdAsync(int userId)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_RoleFunctionPermission_SelectByUserID",
            new SqlParameter("@UserID", userId));
    }

    public async Task<List<Dictionary<string, object?>>> GetByRoleIdAsync(int roleId)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_RoleFunctionPermission_SelectByRoleID",
            new SqlParameter("@RoleId", roleId));
    }

    public async Task<List<MenuFlatDto>> GetMenuFlatByUserIdAsync(int userId)
    {
        List<Dictionary<string, object?>> rows = await GetByUserIdAsync(userId);

        return rows.Select(MapMenuFlatDto).ToList();
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_RoleFunctionPermission_Upsert",
            parameters);
    }

    private static MenuFlatDto MapMenuFlatDto(IReadOnlyDictionary<string, object?> row)
    {
        return new MenuFlatDto
        {
            Id = GetRequiredInt(row, "Id"),
            FunctionId = GetNullableInt(row, "FunctionId"),
            Module = GetNullableString(row, "Module"),
            Name = GetNullableString(row, "Name"),
            Icon = GetNullableString(row, "Icon"),
            Route = GetNullableString(row, "Route"),
            ParentId = GetNullableInt(row, "ParentId"),
            ListPermission = GetNullableString(row, "ListPermission"),
            SortOrder = GetNullableInt(row, "SortOrder")
        };
    }

    private static int GetRequiredInt(IReadOnlyDictionary<string, object?> row, string key)
    {
        object? value = GetValue(row, key);

        if (value is null || value == DBNull.Value)
        {
            throw new InvalidOperationException($"Column '{key}' is required.");
        }

        return Convert.ToInt32(value);
    }

    private static int? GetNullableInt(IReadOnlyDictionary<string, object?> row, string key)
    {
        object? value = GetValue(row, key);

        return value is null || value == DBNull.Value
            ? null
            : Convert.ToInt32(value);
    }

    private static string? GetNullableString(IReadOnlyDictionary<string, object?> row, string key)
    {
        object? value = GetValue(row, key);

        return value is null || value == DBNull.Value
            ? null
            : Convert.ToString(value);
    }

    private static object? GetValue(IReadOnlyDictionary<string, object?> row, string key)
    {
        KeyValuePair<string, object?> item = row.FirstOrDefault(item =>
            string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase));

        return item.Value;
    }
}
