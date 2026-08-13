using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class StudentSessionRepository : BaseRepository
{
    public StudentSessionRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentSession_Select",
            new SqlParameter("@Id", 0));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentSession_Select",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> GetByEnrollmentIdAsync(int enrollmentId)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentSession_SelectByEnrollmentId",
            new SqlParameter("@EnrollmentId", enrollmentId));
    }

    public async Task<List<Dictionary<string, object?>>> GetByUserIdAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentSession_SelectByUserId",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> GetBySessionDateAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentSession_SelectBySessionDate",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> GetTeacherTeachingScheduleAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentSession_SelectTeacherTeachingSchedule",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> GetRoomScheduleAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentSession_SelectRoomSchedule",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> GetCurrentClassAsync(int userId)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentSession_SelectCurrentClass",
            new SqlParameter("@UserID", userId));
    }

    public async Task<List<Dictionary<string, object?>>> UpdateAttendanceAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentSession_UpdateAttendance",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> SyncAttendanceAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentSession_SyncAttendance",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> UpdateMakeupScheduleAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentSession_UpdateMakeupSchedule",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> UpdateEvaluationAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentSession_UpdateEvaluation",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> UpdateTeacherAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentSession_UpdateTeacher",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> RescheduleAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentSession_Reschedule",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> RemoveGapAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentSession_RemoveGap",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> InsertGapAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentSession_InsertGap",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> ChangeGapAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentSession_ChangeGap",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentSession_Upsert",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> DeleteAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_StudentSession_Delete",
            new SqlParameter("@Id", id));
    }
}
