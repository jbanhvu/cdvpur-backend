using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using ChangdaeVinaPurchasingApi.Services;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class RoleFunctionPermissionEndpoints
{
    public static IEndpointRouteBuilder MapRoleFunctionPermissionEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/role-function-permissions")
            .WithTags("RoleFunctionPermissions");

        group.MapGet("", async (RoleFunctionPermissionRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (RoleFunctionPermissionRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapGet("by-user/{userId:int}", async (RoleFunctionPermissionService service, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => service.GetMenuTreeByUserIdAsync(userId));
        });

        group.MapGet("by-role/{roleId:int}", async (RoleFunctionPermissionRepository repository, int roleId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByRoleIdAsync(roleId));
        });

        group.MapPost("", async (RoleFunctionPermissionRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.NullableInt("Id", body),
                    SqlParameterHelper.Int("RoleId", body),
                    SqlParameterHelper.Int("FunctionId", body),
                    SqlParameterHelper.NullableString("ListPermission", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (RoleFunctionPermissionRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.NullableInt("Id", body),
                    SqlParameterHelper.Int("RoleId", body),
                    SqlParameterHelper.Int("FunctionId", body),
                    SqlParameterHelper.NullableString("ListPermission", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        return app;
    }
}
