using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class FeedbackEndpoints
{
    public static IEndpointRouteBuilder MapFeedbackEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/feedback")
            .WithTags("Feedback");

        group.MapGet("", async (FeedbackRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.SelectAsync(0));
        });

        group.MapGet("{id:int}", async (FeedbackRepository repository, int id) =>
        {
            if (id < 0)
            {
                return ApiResponseHelper.Fail("Id must be greater than or equal to 0.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.SelectAsync(id));
        });

        group.MapPost("", async (FeedbackRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Int("Id", body),
                SqlParameterHelper.String("Title", body),
                SqlParameterHelper.String("Content", body),
                SqlParameterHelper.String("CategoryCode", body),
                SqlParameterHelper.String("PriorityCode", body),
                SqlParameterHelper.NullableInt("AssignedUserId", body),
                SqlParameterHelper.Int("UserId", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (FeedbackRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Int("Id", body),
                SqlParameterHelper.String("Title", body),
                SqlParameterHelper.String("Content", body),
                SqlParameterHelper.String("CategoryCode", body),
                SqlParameterHelper.String("PriorityCode", body),
                SqlParameterHelper.NullableInt("AssignedUserId", body),
                SqlParameterHelper.Int("UserId", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPost("update-status", async (FeedbackRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Int("Id", body),
                SqlParameterHelper.String("StatusCode", body),
                SqlParameterHelper.Int("UserId", body)
            ];

            if ((int)parameters[0].Value <= 0)
            {
                return ApiResponseHelper.Fail("Id must be greater than 0.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.UpdateStatusAsync(parameters));
        });

        group.MapGet("{feedbackId:int}/comments", async (FeedbackRepository repository, int feedbackId) =>
        {
            if (feedbackId <= 0)
            {
                return ApiResponseHelper.Fail("FeedbackId must be greater than 0.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.SelectCommentsByFeedbackIdAsync(feedbackId));
        });

        group.MapPost("{feedbackId:int}/comments", async (FeedbackRepository repository, int feedbackId, Dictionary<string, JsonElement> body) =>
        {
            if (feedbackId <= 0)
            {
                return ApiResponseHelper.Fail("FeedbackId must be greater than 0.");
            }

            SqlParameterHelper.SetInt("FeedbackId", body, feedbackId);
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Int("FeedbackId", body),
                SqlParameterHelper.String("Content", body),
                SqlParameterHelper.Int("UserId", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.InsertCommentAsync(parameters));
        });

        return app;
    }
}
