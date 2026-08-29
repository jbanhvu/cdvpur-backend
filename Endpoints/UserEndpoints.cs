using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/users")
            .WithTags("Users");

        group.MapGet("", async (UserRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (UserRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapPost("login", async (UserRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.String("Username", body),
                    SqlParameterHelper.String("PasswordHash", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.LoginAsync(parameters));
        });

        group.MapPost("change-password", async (UserRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.String("OldPasswordHash", body),
                    SqlParameterHelper.String("NewPasswordHash", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.ChangePasswordAsync(parameters));
        });

        group.MapPost("", async (UserRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("Id", body),
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.String("FullName", body),
                    SqlParameterHelper.String("Username", body),
                    SqlParameterHelper.NullableString("PasswordHash", body),
                    SqlParameterHelper.String("Phone", body),
                    SqlParameterHelper.String("Email", body),
                    SqlParameterHelper.Int("RoleID", body),
                    SqlParameterHelper.String("RoleName", body),
                    SqlParameterHelper.NullableInt("BranchId", body),
                    SqlParameterHelper.Bool("IsActive", body),
                    SqlParameterHelper.String("AvatarUrl", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (UserRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("Id", body),
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.String("FullName", body),
                    SqlParameterHelper.String("Username", body),
                    SqlParameterHelper.NullableString("PasswordHash", body),
                    SqlParameterHelper.String("Phone", body),
                    SqlParameterHelper.String("Email", body),
                    SqlParameterHelper.Int("RoleID", body),
                    SqlParameterHelper.String("RoleName", body),
                    SqlParameterHelper.NullableInt("BranchId", body),
                    SqlParameterHelper.Bool("IsActive", body),
                    SqlParameterHelper.String("AvatarUrl", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{id:int}", async (UserRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });

        return app;
    }
}
