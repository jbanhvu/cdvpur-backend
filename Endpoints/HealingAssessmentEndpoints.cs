using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class HealingAssessmentEndpoints
{
    public static IEndpointRouteBuilder MapHealingAssessmentEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/healing-assessments")
            .WithTags("Healing Assessments");

        group.MapGet("{id:int}", async (HealingAssessmentRepository repository, int id) =>
        {
            if (id <= 0)
            {
                return ApiResponseHelper.Fail("Id must be greater than 0.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapGet("by-enrollment/{enrollmentId:int}", async (HealingAssessmentRepository repository, int enrollmentId) =>
        {
            if (enrollmentId <= 0)
            {
                return ApiResponseHelper.Fail("EnrollmentId must be greater than 0.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.GetByEnrollmentIdAsync(enrollmentId));
        });

        group.MapPost("", async (HealingAssessmentRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (HealingAssessmentRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        return app;
    }

    private static SqlParameter[] BuildUpsertParameters(Dictionary<string, JsonElement> body)
    {
        return
        [
            SqlParameterHelper.Int("Id", body),
            SqlParameterHelper.Int("EnrollmentId", body),
            SqlParameterHelper.Int("StudentSessionId", body),
            SqlParameterHelper.Int("TeacherId", body),
            SqlParameterHelper.Date("AssessmentDate", body),
            SqlParameterHelper.Int("SessionNo", body),
            SqlParameterHelper.String("MotorContent", body),
            SqlParameterHelper.String("MotorEvaluation", body),
            SqlParameterHelper.String("CognitiveContent", body),
            SqlParameterHelper.String("CognitiveEvaluation", body),
            SqlParameterHelper.String("LanguageContent", body),
            SqlParameterHelper.String("LanguageEvaluation", body),
            SqlParameterHelper.String("SocialContent", body),
            SqlParameterHelper.String("SocialEvaluation", body),
            SqlParameterHelper.String("FocusContent", body),
            SqlParameterHelper.String("FocusEvaluation", body),
            SqlParameterHelper.String("Note", body),
            SqlParameterHelper.Int("UserId", body)
        ];
    }
}
