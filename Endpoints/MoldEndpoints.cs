using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class MoldEndpoints
{
    public static IEndpointRouteBuilder MapMoldEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/molds")
            .WithTags("Molds");

        group.MapGet("", async (MoldRepository repository, string? keyword, bool? isActive) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetAllAsync(keyword, isActive));
        });

        group.MapGet("{moldId:int}", async (MoldRepository repository, int moldId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(moldId));
        });

        group.MapPost("", async (MoldRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{moldId:int}", async (MoldRepository repository, int moldId, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("MoldId", body, moldId);
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{moldId:int}", async (MoldRepository repository, int moldId, string? userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(moldId, userId));
        });

        return app;
    }

    private static SqlParameter[] BuildUpsertParameters(Dictionary<string, JsonElement> body)
    {
        return
        [
            SqlParameterHelper.NullableInt("MoldId", body),
            SqlParameterHelper.String("MoldCode", body),
            SqlParameterHelper.String("MoldName", body),
            SqlParameterHelper.NullableString("ProductCode", body),
            SqlParameterHelper.NullableString("ProductName", body),
            SqlParameterHelper.NullableBool("IsActive", body),
            SqlParameterHelper.NullableString("Note", body),
            SqlParameterHelper.NullableString("UserId", body)
        ];
    }
}
