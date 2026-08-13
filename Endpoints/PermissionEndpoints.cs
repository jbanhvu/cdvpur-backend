using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class PermissionEndpoints
{
    public static IEndpointRouteBuilder MapPermissionEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/permissions")
            .WithTags("Permissions");

        group.MapGet("", async (PermissionRepository repository, int? id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.SelectAsync(id));
        });

        group.MapPost("", async (PermissionRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.NullableInt("Id", body),
                SqlParameterHelper.String("Code", body),
                SqlParameterHelper.String("Name", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        return app;
    }
}
