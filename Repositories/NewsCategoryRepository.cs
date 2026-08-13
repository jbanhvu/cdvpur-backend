using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class NewsCategoryRepository : BaseRepository
{
    public NewsCategoryRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> SelectAsync(int id = 0)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_NewsCategory_Select",
            new SqlParameter("@Id", id));
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_NewsCategory_Upsert",
            parameters);
    }
}
