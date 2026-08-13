using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Models;
using ChangdaeVinaPurchasingApi.Repositories;
using ChangdaeVinaPurchasingApi.Services;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/notifications")
            .WithTags("Notifications");

        group.MapPost("send-to-user", async (NotificationRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters = BuildSendParameters(body, isMultipleUsers: false);

            return await ApiResponseHelper.HandleAsync(() => repository.SendToUserAsync(parameters));
        });

        group.MapPost("send-to-users", async (NotificationRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters = BuildSendParameters(body, isMultipleUsers: true);

            return await ApiResponseHelper.HandleAsync(() => repository.SendToUsersAsync(parameters));
        });

        group.MapPost("send-to-student", async (NotificationRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters = BuildSendToStudentParameters(body, isMultipleStudents: false);

            return await ApiResponseHelper.HandleAsync(() => repository.SendToStudentAsync(parameters));
        });

        group.MapPost("send-to-students", async (NotificationRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters = BuildSendToStudentParameters(body, isMultipleStudents: true);

            return await ApiResponseHelper.HandleAsync(() => repository.SendToStudentsAsync(parameters));
        });

        group.MapPost("push", async (FirebaseNotificationService service, PushNotificationRequest request) =>
        {
            PushNotificationResponse response = await service.SendToUserAsync(request);
            return Results.Json(response, statusCode: response.Success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest);
        });

        group.MapPost("push-role", async (FirebaseNotificationService service, PushRoleNotificationRequest request) =>
        {
            PushNotificationResponse response = await service.SendToRoleAsync(request);
            return Results.Json(response, statusCode: response.Success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest);
        });

        group.MapPost("test-token", async (FirebaseNotificationService service, TestTokenPushNotificationRequest request) =>
        {
            PushNotificationResponse response = await service.SendToTokenAsync(
                request.Token,
                request.Title,
                request.Body);

            return Results.Json(response, statusCode: response.Success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest);
        });

        return app;
    }

    private static SqlParameter[] BuildSendParameters(IReadOnlyDictionary<string, JsonElement> body, bool isMultipleUsers)
    {
        return
        [
            isMultipleUsers
                ? SqlParameterHelper.String("UserIds", body)
                : SqlParameterHelper.BigInt("UserId", body),
            SqlParameterHelper.Int("TypeId", body),
            SqlParameterHelper.String("Title", body),
            SqlParameterHelper.String("Content", body),
            SqlParameterHelper.String("ImageUrl", body),
            SqlParameterHelper.BigInt("CreatedBy", body),
            SqlParameterHelper.DateTime("ExpiredAt", body)
        ];
    }

    private static SqlParameter[] BuildSendToStudentParameters(IReadOnlyDictionary<string, JsonElement> body, bool isMultipleStudents)
    {
        return
        [
            isMultipleStudents
                ? SqlParameterHelper.String("StudentIds", body)
                : SqlParameterHelper.BigInt("StudentId", body),
            SqlParameterHelper.Int("TypeId", body),
            SqlParameterHelper.String("Title", body),
            SqlParameterHelper.String("Content", body),
            SqlParameterHelper.String("ImageUrl", body),
            SqlParameterHelper.BigInt("CreatedBy", body),
            SqlParameterHelper.DateTime("ExpiredAt", body)
        ];
    }
}
