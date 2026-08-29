using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class HiringDetailEndpoints
{
    public static IEndpointRouteBuilder MapHiringDetailEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/hiring-details")
            .WithTags("Hiring Details");

        group.MapGet("", async (HiringDetailRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (HiringDetailRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapGet("by-hiring/{hiringId:int}", async (HiringDetailRepository repository, int hiringId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByHiringIdAsync(hiringId));
        });

        return app;
    }
}
