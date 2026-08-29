using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class CompanyVisitorEndpoints
{
    public static IEndpointRouteBuilder MapCompanyVisitorEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/company-visitors")
            .WithTags("Company Visitors");

        group.MapGet("", async (CompanyVisitorRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (CompanyVisitorRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        return app;
    }
}
