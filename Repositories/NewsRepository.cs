using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class NewsRepository : BaseRepository
{
    public NewsRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> SelectAsync(int id = 0, int categoryId = 0)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_News_Select",
            new SqlParameter("@Id", id),
            new SqlParameter("@CategoryId", categoryId));
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "Sol_News_Upsert",
            parameters);
    }
}
