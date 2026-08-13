using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class SongApprovalRequestEndpoints
{
    public static IEndpointRouteBuilder MapSongApprovalRequestEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/song-approval-requests")
            .WithTags("Song Approval Requests");

        group.MapGet("", async (SongApprovalRequestRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.SelectAsync(0));
        });

        group.MapGet("{id:int}", async (SongApprovalRequestRepository repository, int id) =>
        {
            if (id < 0)
            {
                return ApiResponseHelper.Fail("Id must be greater than or equal to 0.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.SelectAsync(id));
        });

        group.MapGet("user/{userId:int}", async (SongApprovalRequestRepository repository, int userId) =>
        {
            if (userId <= 0)
            {
                return ApiResponseHelper.Fail("UserId must be greater than 0.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.SelectByUserIdAsync(userId));
        });

        group.MapPost("approval-action-history", async (
            SongApprovalRequestRepository repository,
            Dictionary<string, JsonElement> body) =>
        {
            SqlParameter idParameter = SqlParameterHelper.Int("Id", body);
            SqlParameter userIdParameter = SqlParameterHelper.Int("UserId", body);
            SqlParameter functionIdParameter = SqlParameterHelper.Int("FunctionId", body);

            int id = (int)idParameter.Value;
            int userId = (int)userIdParameter.Value;
            int functionId = (int)functionIdParameter.Value;

            if (id <= 0)
            {
                return ApiResponseHelper.Fail("Id must be greater than 0.");
            }

            if (userId <= 0)
            {
                return ApiResponseHelper.Fail("UserId must be greater than 0.");
            }

            if (functionId <= 0)
            {
                return ApiResponseHelper.Fail("FunctionId must be greater than 0.");
            }

            return await ApiResponseHelper.HandleAsync(() =>
                repository.SelectApprovalActionHistoryAsync(id, userId, functionId));
        });

        group.MapPost("", async (
            SongApprovalRequestRepository repository,
            SongRepository songRepository,
            Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Int("Id", body),
                SqlParameterHelper.Int("StudentId", body),
                SqlParameterHelper.Int("EnrollmentId", body),
                SqlParameterHelper.Int("StudentSessionId", body),
                SqlParameterHelper.Int("SongId", body),
                SqlParameterHelper.NullableString("Reason", body),
                SqlParameterHelper.NullableString("Note", body),
                SqlParameterHelper.String("StatusCode", body),
                SqlParameterHelper.Int("CurrentStepOrder", body),
                SqlParameterHelper.NullableDateTime("CompletedOn", body),
                SqlParameterHelper.Int("UserId", body)
            ];

            if ((int)parameters[0].Value < 0)
            {
                return ApiResponseHelper.Fail("Id must be greater than or equal to 0.");
            }

            int songId = (int)parameters[4].Value;
            if (songId <= 0)
            {
                return ApiResponseHelper.Fail("SongId must be greater than 0.");
            }

            if (string.IsNullOrWhiteSpace(parameters[7].Value?.ToString()))
            {
                return ApiResponseHelper.Fail("StatusCode is required.");
            }

            return await ApiResponseHelper.HandleAsync(async () =>
            {
                List<Dictionary<string, object?>> songs = await songRepository.SelectAsync(songId);
                if (songs.Count == 0)
                {
                    throw new ArgumentException($"SongId '{songId}' does not exist.");
                }

                return await repository.UpsertAsync(parameters);
            });
        });

        return app;
    }
}
