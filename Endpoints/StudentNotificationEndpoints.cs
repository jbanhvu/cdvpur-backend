using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class StudentNotificationEndpoints
{
    public static IEndpointRouteBuilder MapStudentNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/student-notifications")
            .WithTags("Student Notifications");

        group.MapPost("select-by-student", async (NotificationRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.BigInt("StudentId", body),
                SqlParameterHelper.NullableBool("IsRead", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.SelectByStudentIdAsync(parameters));
        });

        group.MapPost("{id:long}/click", async (NotificationRepository repository, long id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.ClickStudentNotificationAsync(id));
        });

        group.MapPost("{id:long}/delete", async (NotificationRepository repository, long id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteStudentNotificationAsync(id));
        });

        group.MapPost("{studentId:long}/unread-count", async (NotificationRepository repository, long studentId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetStudentUnreadCountAsync(studentId));
        });

        group.MapPost("{studentId:long}/mark-all-as-read", async (NotificationRepository repository, long studentId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.MarkAllStudentAsReadAsync(studentId));
        });

        group.MapPost("{id:long}/mark-as-read", async (NotificationRepository repository, long id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.MarkStudentAsReadAsync(id));
        });

        return app;
    }
}
