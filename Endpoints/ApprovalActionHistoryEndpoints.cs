using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class ApprovalActionHistoryEndpoints
{
    public static IEndpointRouteBuilder MapApprovalActionHistoryEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/approval-action-history")
            .WithTags("Approval Action History");

        group.MapGet("", async (ApprovalActionHistoryRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.SelectAsync(0));
        });

        group.MapGet("{id:int}", async (ApprovalActionHistoryRepository repository, int id) =>
        {
            if (id < 0)
            {
                return ApiResponseHelper.Fail("Id must be greater than or equal to 0.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.SelectAsync(id));
        });

        group.MapPost("", async (ApprovalActionHistoryRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.NullableInt("Id", body, 0),
                SqlParameterHelper.Int("RequestId", body),
                SqlParameterHelper.Int("WorkflowStepId", body),
                SqlParameterHelper.String("ActionCode", body),
                SqlParameterHelper.NullableString("Comment", body),
                SqlParameterHelper.Int("ActionBy", body),
                SqlParameterHelper.Int("FunctionId", body)
            ];

            if ((int)parameters[0].Value < 0)
            {
                return ApiResponseHelper.Fail("Id must be greater than or equal to 0.");
            }

            if (string.IsNullOrWhiteSpace(parameters[3].Value?.ToString()))
            {
                return ApiResponseHelper.Fail("ActionCode is required.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        return app;
    }
}
