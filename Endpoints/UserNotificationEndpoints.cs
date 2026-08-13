using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class UserNotificationEndpoints
{
    public static IEndpointRouteBuilder MapUserNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/user-notifications")
            .WithTags("User Notifications");

        group.MapPost("select-by-user", async (NotificationRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.BigInt("UserId", body),
                SqlParameterHelper.NullableBool("IsRead", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.SelectByUserIdAsync(parameters));
        });

        group.MapPost("{id:long}/click", async (NotificationRepository repository, long id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.ClickUserNotificationAsync(id));
        });

        group.MapPost("{id:long}/delete", async (NotificationRepository repository, long id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteUserNotificationAsync(id));
        });

        group.MapPost("{userId:long}/unread-count", async (NotificationRepository repository, long userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetUnreadCountAsync(userId));
        });

        group.MapPost("{userId:long}/mark-all-as-read", async (NotificationRepository repository, long userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.MarkAllAsReadAsync(userId));
        });

        group.MapPost("{id:long}/mark-as-read", async (NotificationRepository repository, long id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.MarkAsReadAsync(id));
        });

        return app;
    }
}
