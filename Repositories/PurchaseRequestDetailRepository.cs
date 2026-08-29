using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class PurchaseRequestDetailRepository : BaseRepository
{
    public PurchaseRequestDetailRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_PurchaseRequestDetail_Select",
            new SqlParameter("@Id", DBNull.Value),
            new SqlParameter("@PurchaseRequestId", DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_PurchaseRequestDetail_Select",
            new SqlParameter("@Id", id),
            new SqlParameter("@PurchaseRequestId", DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> GetByPurchaseRequestIdAsync(int purchaseRequestId)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_PurchaseRequestDetail_Select",
            new SqlParameter("@Id", DBNull.Value),
            new SqlParameter("@PurchaseRequestId", purchaseRequestId));
    }

    public async Task<List<Dictionary<string, object?>>> BulkSaveAsync(int purchaseRequestId, DataTable details)
    {
        SqlParameter detailsParameter = new("@Details", SqlDbType.Structured)
        {
            TypeName = "nhvpa3en_vpa01.CDV_PurchaseRequestDetailType",
            Value = details
        };

        return await ExecuteStoredProcedureAsync(
            "CDV_PurchaseRequestDetail_Upsert",
            new SqlParameter("@PurchaseRequestId", purchaseRequestId),
            detailsParameter);
    }
}
