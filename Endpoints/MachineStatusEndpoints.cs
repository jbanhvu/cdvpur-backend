using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class MachineStatusEndpoints
{
    public static IEndpointRouteBuilder MapMachineStatusEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/machine-statuses")
            .WithTags("Machine Statuses");

        group.MapGet("", async (MachineStatusRepository repository, bool? isActive) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetAllAsync(isActive));
        });

        return app;
    }
}
