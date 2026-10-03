using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using ChangdaeVinaPurchasingApi.Services.Adsun;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class AdsunGpsEndpoints
{
    public static IEndpointRouteBuilder MapAdsunGpsEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/adsun/gps")
            .WithTags("ADSUN GPS");

        group.MapGet("latest", async (VehicleGpsLogRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetLatestAsync(null));
        });

        group.MapGet("latest/{vehicleId:int}", async (VehicleGpsLogRepository repository, int vehicleId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetLatestAsync(vehicleId));
        });

        group.MapGet("history/{vehicleId:int}", async (
            VehicleGpsLogRepository repository,
            int vehicleId,
            DateTime? fromTime,
            DateTime? toTime) =>
        {
            if (fromTime is null || toTime is null)
            {
                return ApiResponseHelper.Fail("Query parameters 'fromTime' and 'toTime' are required.");
            }

            if (toTime < fromTime)
            {
                return ApiResponseHelper.Fail("Time range is invalid.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.GetHistoryAsync(vehicleId, fromTime.Value, toTime.Value));
        });

        group.MapPost("sync", async (IAdsunService adsunService) =>
        {
            return await ApiResponseHelper.HandleAsync(() => adsunService.SyncGpsAsync());
        });

        return app;
    }
}
