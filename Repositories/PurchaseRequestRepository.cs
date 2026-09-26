using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class PurchaseRequestRepository : BaseRepository
{
    public PurchaseRequestRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_PurchaseRequest_Select",
            new SqlParameter("@Id", 0));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_PurchaseRequest_Select",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> GetByDepartmentNameAsync(string? departmentName)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_PurchaseRequest_SelectByDepartmentName",
            new SqlParameter("@DepartmentName", string.IsNullOrWhiteSpace(departmentName) ? DBNull.Value : departmentName));
    }
}
