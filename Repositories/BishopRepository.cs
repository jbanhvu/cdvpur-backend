using ChangdaeVinaPurchasingApi.Base;
using ChangdaeVinaPurchasingApi.Database;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class BishopRepository : BaseRepository
{
    public BishopRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await ExecuteStoredProcedureAsync(
            StoredProcedureNames.BishopsSelect,
            new SqlParameter("@ID", -1));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            StoredProcedureNames.BishopsSelect,
            new SqlParameter("@ID", id));
    }

    public async Task<List<Dictionary<string, object?>>> InsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            StoredProcedureNames.BishopsInsert,
            parameters);
    }
}
