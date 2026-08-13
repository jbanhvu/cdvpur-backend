using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class UserFCMTokenEndpoints
{
    public static IEndpointRouteBuilder MapUserFCMTokenEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/user-fcm-tokens")
            .WithTags("User FCM Tokens");

        group.MapGet("user/{userId:int}", async (UserFCMTokenRepository repository, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByUserIdAsync(userId));
        });

        group.MapGet("user-token/{userId:int}", async (UserFCMTokenRepository repository, int userId, string? userType) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByUserAsync(userId, userType));
        });

        group.MapPost("role", async (UserFCMTokenRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.NullableInt("RoleID", body, 0),
                SqlParameterHelper.NullableInt("BranchId", body, 0)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.GetByRoleAsync(parameters));
        });

        group.MapPost("", async (UserFCMTokenRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters = CreateUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (UserFCMTokenRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters = CreateUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPost("delete", async (UserFCMTokenRepository repository, Dictionary<string, JsonElement> body) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(GetToken(body)));
        });

        group.MapDelete("token/{token}", async (UserFCMTokenRepository repository, string token) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(token));
        });

        return app;
    }

    private static SqlParameter[] CreateUpsertParameters(Dictionary<string, JsonElement> body)
    {
        return
        [
            SqlParameterHelper.Int("Id", body),
            SqlParameterHelper.Int("UserId", body),
            SqlParameterHelper.String("Token", body),
            SqlParameterHelper.String("DeviceId", body),
            SqlParameterHelper.String("DeviceType", body),
            SqlParameterHelper.String("DeviceName", body),
            SqlParameterHelper.String("Browser", body),
            SqlParameterHelper.DateTime("LastActive", body),
            SqlParameterHelper.DateTime("CreatedDate", body),
            SqlParameterHelper.String("UserType", body)
        ];
    }

    private static string GetToken(IReadOnlyDictionary<string, JsonElement> body)
    {
        foreach (KeyValuePair<string, JsonElement> item in body)
        {
            if (string.Equals(item.Key, "Token", StringComparison.OrdinalIgnoreCase))
            {
                return item.Value.ValueKind == JsonValueKind.String
                    ? item.Value.GetString() ?? string.Empty
                    : item.Value.ToString();
            }
        }

        throw new ArgumentException("Missing required field 'Token'.");
    }
}
