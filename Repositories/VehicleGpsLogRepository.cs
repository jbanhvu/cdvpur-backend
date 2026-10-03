using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class VehicleGpsLogRepository : BaseRepository
{
    public VehicleGpsLogRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> SyncAsync(string gpsJson)
    {
        return await ExecuteStoredProcedureAsync(
            "nhvpa3en_vpa01.CDV_sp_VehicleGPSLog_Sync",
            new SqlParameter("@GpsJson", gpsJson));
    }

    public async Task<List<Dictionary<string, object?>>> GetLatestAsync(int? vehicleId)
    {
        return await ExecuteStoredProcedureAsync(
            "nhvpa3en_vpa01.CDV_sp_VehicleGPSLog_GetLatest",
            new SqlParameter("@VehicleId", (object?)vehicleId ?? DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> GetHistoryAsync(int vehicleId, DateTime fromTime, DateTime toTime)
    {
        return await ExecuteStoredProcedureAsync(
            "nhvpa3en_vpa01.CDV_sp_VehicleGPSLog_GetHistory",
            new SqlParameter("@VehicleId", vehicleId),
            new SqlParameter("@FromTime", fromTime),
            new SqlParameter("@ToTime", toTime));
    }
}
