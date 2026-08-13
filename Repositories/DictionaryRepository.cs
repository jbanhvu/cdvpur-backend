using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class DictionaryRepository : BaseRepository
{
    public DictionaryRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_Dictionary_GetByID",
            new SqlParameter("@ID", id));
    }

    public async Task<List<Dictionary<string, object?>>> GetByModuleAsync(string module)
    {
        SqlParameter moduleParameter = new("@Module", SqlDbType.NVarChar, 100)
        {
            Value = module
        };

        return await ExecuteStoredProcedureAsync(
            "CDV_Dictionary_GetByModule",
            moduleParameter);
    }
}
