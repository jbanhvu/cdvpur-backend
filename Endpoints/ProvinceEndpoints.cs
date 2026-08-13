using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class ProvinceEndpoints
{
    public static IEndpointRouteBuilder MapProvinceEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/provinces")
            .WithTags("Provinces");

        group.MapGet("", async (ProvinceRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        return app;
    }
}
