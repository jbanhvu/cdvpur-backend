using ChangdaeVinaPurchasingApi.Base;
using ChangdaeVinaPurchasingApi.Database;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class ProvinceRepository : BaseRepository
{
    public ProvinceRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await ExecuteStoredProcedureAsync(
            StoredProcedureNames.ProvinceSelect,
            new SqlParameter("@ID", 0));
    }
}
