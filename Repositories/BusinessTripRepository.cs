using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class BusinessTripRepository : BaseRepository
{
    public BusinessTripRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_BusinessTrip_Select",
            new SqlParameter("@Id", 0));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_BusinessTrip_Select",
            new SqlParameter("@Id", id));
    }
}
