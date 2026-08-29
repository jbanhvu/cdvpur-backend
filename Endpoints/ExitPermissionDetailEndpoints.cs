using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class ExitPermissionDetailEndpoints
{
    public static IEndpointRouteBuilder MapExitPermissionDetailEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/exit-permission-details")
            .WithTags("Exit Permission Details");

        group.MapGet("", async (ExitPermissionDetailRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (ExitPermissionDetailRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapGet("by-exit-permission/{exitPermissionId:int}", async (ExitPermissionDetailRepository repository, int exitPermissionId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByExitPermissionIdAsync(exitPermissionId));
        });

        return app;
    }
}
