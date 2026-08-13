using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class NotificationTypeEndpoints
{
    public static IEndpointRouteBuilder MapNotificationTypeEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/notification-types")
            .WithTags("Notification Types");

        group.MapPost("{id:int}", async (NotificationRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.SelectTypeAsync(id));
        });

        group.MapPost("", async (NotificationRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Int("Id", body),
                SqlParameterHelper.Int("UserId", body),
                SqlParameterHelper.String("Name", body),
                SqlParameterHelper.String("Icon", body),
                SqlParameterHelper.String("Color", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertTypeAsync(parameters));
        });

        return app;
    }
}
