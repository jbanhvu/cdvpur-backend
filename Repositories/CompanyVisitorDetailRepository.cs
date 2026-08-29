using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class CompanyVisitorDetailRepository : BaseRepository
{
    public CompanyVisitorDetailRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_CompanyVisitorDetail_Select",
            new SqlParameter("@Id", 0),
            new SqlParameter("@CompanyVisitorId", DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_CompanyVisitorDetail_Select",
            new SqlParameter("@Id", id),
            new SqlParameter("@CompanyVisitorId", DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> GetByCompanyVisitorIdAsync(int companyVisitorId)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_CompanyVisitorDetail_Select",
            new SqlParameter("@Id", 0),
            new SqlParameter("@CompanyVisitorId", companyVisitorId));
    }
}
