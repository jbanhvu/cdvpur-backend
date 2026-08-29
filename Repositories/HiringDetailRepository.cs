using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class HiringDetailRepository : BaseRepository
{
    public HiringDetailRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_HiringDetail_Select",
            new SqlParameter("@Id", 0),
            new SqlParameter("@HiringId", DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_HiringDetail_Select",
            new SqlParameter("@Id", id),
            new SqlParameter("@HiringId", DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> GetByHiringIdAsync(int hiringId)
    {
        return await ExecuteStoredProcedureAsync(
            "CDV_HiringDetail_Select",
            new SqlParameter("@Id", 0),
            new SqlParameter("@HiringId", hiringId));
    }
}
