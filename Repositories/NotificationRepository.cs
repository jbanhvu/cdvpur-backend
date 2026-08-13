using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class NotificationRepository : BaseRepository
{
    public NotificationRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> SendToUserAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Notification_SendToUser",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> SendToUsersAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Notification_SendToUsers",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> SendToStudentAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Notification_SendToStudent",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> SendToStudentsAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Notification_SendToStudents",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> SelectTypeAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_NotificationType_Select",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> UpsertTypeAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_NotificationType_Upsert",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> ClickUserNotificationAsync(long id)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_UserNotification_Click",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> DeleteUserNotificationAsync(long id)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_UserNotification_Delete",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> GetUnreadCountAsync(long userId)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_UserNotification_GetUnreadCount",
            new SqlParameter("@UserId", userId));
    }

    public async Task<List<Dictionary<string, object?>>> MarkAllAsReadAsync(long userId)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_UserNotification_MarkAllAsRead",
            new SqlParameter("@UserId", userId));
    }

    public async Task<List<Dictionary<string, object?>>> MarkAsReadAsync(long id)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_UserNotification_MarkAsRead",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> SelectByUserIdAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_UserNotification_SelectByUserId",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> ClickStudentNotificationAsync(long id)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentNotification_Click",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> DeleteStudentNotificationAsync(long id)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentNotification_Delete",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> GetStudentUnreadCountAsync(long studentId)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentNotification_GetUnreadCount",
            new SqlParameter("@StudentId", studentId));
    }

    public async Task<List<Dictionary<string, object?>>> MarkAllStudentAsReadAsync(long studentId)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentNotification_MarkAllAsRead",
            new SqlParameter("@StudentId", studentId));
    }

    public async Task<List<Dictionary<string, object?>>> MarkStudentAsReadAsync(long id)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentNotification_MarkAsRead",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> SelectByStudentIdAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentNotification_SelectByStudentId",
            parameters);
    }
}
