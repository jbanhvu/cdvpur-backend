using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class RecycleOutEndpoints
{
    public static IEndpointRouteBuilder MapRecycleOutEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/recycle-outs")
            .WithTags("Recycle Outs");

        group.MapGet("", async (RecycleOutRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (RecycleOutRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapPost("", async (RecycleOutRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (RecycleOutRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{id:int}", async (RecycleOutRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });

        return app;
    }

    private static SqlParameter[] BuildUpsertParameters(Dictionary<string, JsonElement> body)
    {
        return
        [
            SqlParameterHelper.NullableInt("Id", body, -1),
            SqlParameterHelper.NullableInt("UserId", body, 0),
            SqlParameterHelper.NullableString("RecycleOutNo", body),
            SqlParameterHelper.Int("SupplierId", body),
            SqlParameterHelper.DateTime("ExportDate", body),
            SqlParameterHelper.String("Status", body),
            SqlParameterHelper.NullableString("Note", body)
        ];
    }
}
