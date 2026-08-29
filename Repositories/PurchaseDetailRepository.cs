using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class PurchaseDetailRepository : BaseRepository
{
    public PurchaseDetailRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_PurchaseDetail_Select",
            new SqlParameter("@Id", DBNull.Value),
            new SqlParameter("@PurchaseId", DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_PurchaseDetail_Select",
            new SqlParameter("@Id", id),
            new SqlParameter("@PurchaseId", DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> GetByPurchaseIdAsync(int purchaseId)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_PurchaseDetail_Select",
            new SqlParameter("@Id", DBNull.Value),
            new SqlParameter("@PurchaseId", purchaseId));
    }
}
