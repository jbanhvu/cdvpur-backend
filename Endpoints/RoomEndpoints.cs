using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class RoomEndpoints
{
    public static IEndpointRouteBuilder MapRoomEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/rooms")
            .WithTags("Rooms");

        group.MapGet("", async (RoomRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (RoomRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapPost("", async (RoomRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.NullableInt("Id", body, -1),
                    SqlParameterHelper.NullableInt("UserId", body, 0),
                    SqlParameterHelper.Int("BranchId", body),
                    SqlParameterHelper.String("Name", body),
                    SqlParameterHelper.Int("Capacity", body),
                    SqlParameterHelper.String("Description", body),
                    SqlParameterHelper.String("HexColor", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (RoomRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.NullableInt("Id", body, -1),
                    SqlParameterHelper.NullableInt("UserId", body, 0),
                    SqlParameterHelper.Int("BranchId", body),
                    SqlParameterHelper.String("Name", body),
                    SqlParameterHelper.Int("Capacity", body),
                    SqlParameterHelper.String("Description", body),
                    SqlParameterHelper.String("HexColor", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{id:int}", async (RoomRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });

        return app;
    }
}
