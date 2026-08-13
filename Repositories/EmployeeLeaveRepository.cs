using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class EmployeeLeaveRepository : BaseRepository
{
    public EmployeeLeaveRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetSummaryAsync(
        DateTime fromDate,
        DateTime toDate,
        string? departmentName)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_EmployeeLeave_Summary",
            new SqlParameter("@FromDate", SqlDbType.Date) { Value = fromDate.Date },
            new SqlParameter("@ToDate", SqlDbType.Date) { Value = toDate.Date },
            new SqlParameter("@DepartmentName", SqlDbType.NVarChar, 200)
            {
                Value = string.IsNullOrWhiteSpace(departmentName) ? DBNull.Value : departmentName
            });
    }

    public async Task<List<Dictionary<string, object?>>> SelectAsync(
        DateTime? fromDate,
        DateTime? toDate,
        string? approvalStatus)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.CDV_EmployeeLeave_Select",
            new SqlParameter("@FromDate", SqlDbType.Date)
            {
                Value = fromDate.HasValue ? fromDate.Value.Date : DBNull.Value
            },
            new SqlParameter("@ToDate", SqlDbType.Date)
            {
                Value = toDate.HasValue ? toDate.Value.Date : DBNull.Value
            },
            new SqlParameter("@ApprovalStatus", SqlDbType.NVarChar, 50)
            {
                Value = string.IsNullOrWhiteSpace(approvalStatus) ? DBNull.Value : approvalStatus
            });
    }
}
