using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class PurchaseRepository : BaseRepository
{
    public PurchaseRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_Purchase_Select",
            new SqlParameter("@Id", 0));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_Purchase_Select",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> GetPu02ByPu01Async()
    {
        return await ExecuteStoredProcedureAsync("CDV_Purchase_Get_PU02_By_PU01");
    }
}
