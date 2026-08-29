using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class CompanyVisitorDetailEndpoints
{
    public static IEndpointRouteBuilder MapCompanyVisitorDetailEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/company-visitor-details")
            .WithTags("Company Visitor Details");

        group.MapGet("", async (CompanyVisitorDetailRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (CompanyVisitorDetailRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapGet("by-company-visitor/{companyVisitorId:int}", async (CompanyVisitorDetailRepository repository, int companyVisitorId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByCompanyVisitorIdAsync(companyVisitorId));
        });

        return app;
    }
}
