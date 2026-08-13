using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class ApprovalWorkflowStepEndpoints
{
    public static IEndpointRouteBuilder MapApprovalWorkflowStepEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/approval-workflow-steps")
            .WithTags("Approval Workflow Steps");

        group.MapGet("{id:int}", async (ApprovalWorkflowStepRepository repository, int id) =>
        {
            if (id < 0)
            {
                return ApiResponseHelper.Fail("Id must be greater than or equal to 0.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.SelectAsync(id));
        });

        group.MapPost("", async (ApprovalWorkflowStepRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                SqlParameterHelper.Int("Id", body),
                SqlParameterHelper.String("ApprovalTypeCode", body),
                SqlParameterHelper.Int("StepOrder", body),
                SqlParameterHelper.Int("RoleId", body),
                SqlParameterHelper.NullableString("StepName", body),
                SqlParameterHelper.Bool("IsRequired", body),
                SqlParameterHelper.Bool("IsActive", body),
                SqlParameterHelper.Int("SortOrder", body),
                SqlParameterHelper.Int("UserId", body)
            ];

            if ((int)parameters[0].Value < 0)
            {
                return ApiResponseHelper.Fail("Id must be greater than or equal to 0.");
            }

            if (string.IsNullOrWhiteSpace(parameters[1].Value?.ToString()))
            {
                return ApiResponseHelper.Fail("ApprovalTypeCode is required.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        return app;
    }
}
