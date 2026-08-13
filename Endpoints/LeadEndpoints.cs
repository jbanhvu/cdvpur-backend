using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class LeadEndpoints
{
    public static IEndpointRouteBuilder MapLeadEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/leads")
            .WithTags("Leads");

        group.MapGet("", async (
            LeadRepository repository,
            string? keyword,
            string? status,
            int? receivedByUserId,
            int? consultedByUserId,
            DateTime? fromDate,
            DateTime? toDate) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.SelectAsync(
                -1,
                keyword,
                status,
                receivedByUserId,
                consultedByUserId,
                fromDate,
                toDate));
        });

        group.MapGet("{id:int}", async (LeadRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapPost("", async (LeadRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (LeadRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters = BuildUpsertParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapGet("{leadId:int}/logs", async (LeadRepository repository, int leadId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetLogsAsync(leadId));
        });

        group.MapGet("logs/{id:int}", async (LeadRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetLogByIdAsync(id));
        });

        group.MapPost("{leadId:int}/logs", async (LeadRepository repository, int leadId, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("LeadId", body, leadId);
            SqlParameter[] parameters = BuildUpsertLogParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertLogAsync(parameters));
        });

        group.MapPut("logs/{id:int}", async (LeadRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters = BuildUpsertLogParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertLogAsync(parameters));
        });

        RouteGroupBuilder logGroup = app.MapGroup("/api/lead-logs")
            .WithTags("Lead Logs");

        logGroup.MapGet("", async (LeadRepository repository, int? leadId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.SelectLogsAsync(0, leadId));
        });

        logGroup.MapGet("{id:int}", async (LeadRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetLogByIdAsync(id));
        });

        logGroup.MapPost("", async (LeadRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters = BuildUpsertLogParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertLogAsync(parameters));
        });

        logGroup.MapPut("{id:int}", async (LeadRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters = BuildUpsertLogParameters(body);

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertLogAsync(parameters));
        });

        return app;
    }

    private static SqlParameter[] BuildUpsertParameters(IReadOnlyDictionary<string, JsonElement> body)
    {
        return
        [
            SqlParameterHelper.NullableInt("Id", body, 0),
            SqlParameterHelper.NullableString("Code", body),
            SqlParameterHelper.NullableString("FullName", body),
            SqlParameterHelper.NullableString("Gender", body),
            SqlParameterHelper.NullableInt("BirthYear", body),
            SqlParameterHelper.NullableString("StudentPhone", body),
            SqlParameterHelper.NullableString("ParentName", body),
            SqlParameterHelper.NullableString("ParentPhone", body),
            SqlParameterHelper.NullableString("Address", body),
            SqlParameterHelper.NullableString("CourseDesc", body),
            SqlParameterHelper.NullableString("LevelDesc", body),
            SqlParameterHelper.NullableString("ExpectedSchedule", body),
            SqlParameterHelper.NullableDate("ExpectedStartDate", body),
            SqlParameterHelper.NullableInt("ReceivedByUserId", body),
            SqlParameterHelper.NullableDateTime("ReceivedDate", body),
            SqlParameterHelper.NullableString("Status", body),
            SqlParameterHelper.NullableInt("ConsultedByUserId", body),
            SqlParameterHelper.NullableDateTime("ConsultedDate", body),
            SqlParameterHelper.NullableDateTime("AppointmentDate", body),
            SqlParameterHelper.NullableDateTime("EnrollmentDate", body),
            SqlParameterHelper.NullableInt("StudentId", body),
            SqlParameterHelper.NullableString("LeadSource", body),
            SqlParameterHelper.NullableString("Note", body),
            SqlParameterHelper.NullableInt("CreatedBy", body),
            SqlParameterHelper.NullableInt("UpdatedBy", body)
        ];
    }

    private static SqlParameter[] BuildUpsertLogParameters(IReadOnlyDictionary<string, JsonElement> body)
    {
        return
        [
            SqlParameterHelper.NullableInt("Id", body, 0),
            SqlParameterHelper.NullableInt("LeadId", body),
            SqlParameterHelper.NullableString("ActionType", body),
            SqlParameterHelper.NullableString("Content", body),
            SqlParameterHelper.NullableDateTime("NextFollowDate", body),
            SqlParameterHelper.NullableInt("CreatedBy", body)
        ];
    }
}
