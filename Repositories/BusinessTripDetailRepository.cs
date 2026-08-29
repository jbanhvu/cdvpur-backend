using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class BusinessTripDetailRepository : BaseRepository
{
    public BusinessTripDetailRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_BusinessTripDetail_Select",
            new SqlParameter("@Id", 0),
            new SqlParameter("@BusinessTripId", DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_BusinessTripDetail_Select",
            new SqlParameter("@Id", id),
            new SqlParameter("@BusinessTripId", DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> GetByBusinessTripIdAsync(int businessTripId)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_BusinessTripDetail_Select",
            new SqlParameter("@Id", 0),
            new SqlParameter("@BusinessTripId", businessTripId));
    }
}
