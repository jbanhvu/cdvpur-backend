using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class LeadRepository : BaseRepository
{
    public LeadRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await SelectAsync(0, null, null, null, null, null, null);
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await SelectAsync(id, null, null, null, null, null, null);
    }

    public async Task<List<Dictionary<string, object?>>> SelectAsync(
        int id,
        string? keyword,
        string? status,
        int? receivedByUserId,
        int? consultedByUserId,
        DateTime? fromDate,
        DateTime? toDate)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Lead_Select",
            new SqlParameter("@Id", id),
            new SqlParameter("@Keyword", string.IsNullOrWhiteSpace(keyword) ? DBNull.Value : keyword),
            new SqlParameter("@Status", string.IsNullOrWhiteSpace(status) ? DBNull.Value : status),
            new SqlParameter("@ReceivedByUserId", receivedByUserId ?? (object)DBNull.Value),
            new SqlParameter("@ConsultedByUserId", consultedByUserId ?? (object)DBNull.Value),
            new SqlParameter("@FromDate", fromDate?.Date ?? (object)DBNull.Value),
            new SqlParameter("@ToDate", toDate?.Date ?? (object)DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_Lead_Upsert",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> GetLogsAsync(int leadId)
    {
        return await SelectLogsAsync(null, leadId);
    }

    public async Task<List<Dictionary<string, object?>>> GetLogByIdAsync(int id, int leadId = 0)
    {
        return await SelectLogsAsync(id, leadId);
    }

    public async Task<List<Dictionary<string, object?>>> SelectLogsAsync(int? id, int? leadId)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_LeadLog_Select",
            new SqlParameter("@Id", id),
            new SqlParameter("@LeadId", leadId ?? (object)DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> UpsertLogAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_LeadLog_Upsert",
            parameters);
    }
}
