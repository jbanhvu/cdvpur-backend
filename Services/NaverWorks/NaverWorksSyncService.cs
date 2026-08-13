using ChangdaeVinaPurchasingApi.Models.NaverWorks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;
using System.Globalization;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Services.NaverWorks;

public class NaverWorksSyncService
{
    private const string LeaveTypeComponentId = "a43c02d9-9dca-0fd7-6566-284140e31f71";
    private const string LeavePeriodComponentId = "a999e1d5-df77-fe62-0f2b-82a880f83d17";
    private const string LeaveDaysComponentId = "ff857af5-26f9-8f95-7bd5-9a8c196c07f5";
    private const string TimeUsedComponentId = "1ccae4ee-9ea0-b5eb-1f14-3eacf6a18546";
    private const string DeductHoursComponentId = "0d3cc565-077f-fe1c-0bd2-74be443fe28d";
    private const string ReasonComponentId = "b4f34a15-6fc8-81a1-5266-4d317cf5ad82";

    private readonly NaverWorksApprovalService _approvalService;
    private readonly IConfiguration _configuration;
    private readonly IOptions<NaverWorksOptions> _options;
    private readonly ILogger<NaverWorksSyncService> _logger;

    public NaverWorksSyncService(
        NaverWorksApprovalService approvalService,
        IConfiguration configuration,
        IOptions<NaverWorksOptions> options,
        ILogger<NaverWorksSyncService> logger)
    {
        _approvalService = approvalService;
        _configuration = configuration;
        _options = options;
        _logger = logger;
    }

    public async Task<NaverWorksSyncResult> SyncLeaveAsync(
        DateTime fromDate,
        DateTime untilDate,
        CancellationToken cancellationToken = default)
    {
        string leaveDocumentFormId = _options.Value.LeaveDocumentFormId;
        if (string.IsNullOrWhiteSpace(leaveDocumentFormId))
        {
            throw new InvalidOperationException("NaverWorks:LeaveDocumentFormId is missing.");
        }

        _logger.LogInformation("Starting NAVER WORKS leave sync from {FromDate} to {UntilDate}.", fromDate, untilDate);

        List<NaverWorksApprovalDocument> documents = await _approvalService.GetApprovalDocumentsAsync(
            fromDate,
            untilDate,
            cancellationToken);

        List<NaverWorksApprovalDocument> leaveDocuments = documents
            .Where(document => string.Equals(document.DocumentFormId, leaveDocumentFormId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        NaverWorksSyncResult result = new()
        {
            TotalDocuments = documents.Count,
            LeaveDocuments = leaveDocuments.Count
        };

        _logger.LogInformation(
            "NAVER WORKS returned {TotalDocuments} documents, {LeaveDocuments} leave documents.",
            result.TotalDocuments,
            result.LeaveDocuments);

        foreach (NaverWorksApprovalDocument document in leaveDocuments)
        {
            try
            {
                _logger.LogInformation("Syncing NAVER WORKS approvalDocumentId {ApprovalDocumentId}.", document.ApprovalDocumentId);

                JsonElement detailJson = await _approvalService.GetApprovalDocumentDetailJsonAsync(
                    document.ApprovalDocumentId,
                    cancellationToken);

                NaverWorksApprovalDetailResponse detail = _approvalService.ParseApprovalDetail(detailJson);
                string rawJson = detailJson.GetRawText();
                ParsedLeaveBody parsedLeaveBody = ParseLeaveBody(detail.DocumentBody);

                await using SqlConnection connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);

                await SaveRawAsync(connection, detail, rawJson, cancellationToken);
                await SaveEmployeeLeaveAsync(connection, detail, parsedLeaveBody, cancellationToken);

                result.Synced++;
            }
            catch (Exception ex)
            {
                result.Failed++;
                string error = $"{document.ApprovalDocumentId}: {ex.Message}";
                result.Errors.Add(error);
                _logger.LogError(ex, "Failed to sync NAVER WORKS approvalDocumentId {ApprovalDocumentId}.", document.ApprovalDocumentId);
            }
        }

        _logger.LogInformation(
            "Finished NAVER WORKS leave sync. Synced={Synced}, Failed={Failed}.",
            result.Synced,
            result.Failed);

        return result;
    }

    private static ParsedLeaveBody ParseLeaveBody(List<NaverWorksDocumentComponent> components)
    {
        NaverWorksDocumentComponent? leaveType = FindComponent(components, LeaveTypeComponentId);
        NaverWorksDocumentComponent? leavePeriod = FindComponent(components, LeavePeriodComponentId);
        NaverWorksDocumentComponent? leaveDays = FindComponent(components, LeaveDaysComponentId);
        NaverWorksDocumentComponent? timeUsed = FindComponent(components, TimeUsedComponentId);
        NaverWorksDocumentComponent? deductHours = FindComponent(components, DeductHoursComponentId);
        NaverWorksDocumentComponent? reason = FindComponent(components, ReasonComponentId);

        return new ParsedLeaveBody
        {
            LeaveType = ReadItemName(leaveType?.ComponentValue),
            FromDate = ReadNaverDate(leavePeriod?.ComponentValue, "startDate"),
            ToDate = ReadNaverDate(leavePeriod?.ComponentValue, "endDate"),
            LeaveDays = ReadDecimal(leaveDays?.ComponentValue, "value"),
            TimeUsed = ReadString(timeUsed?.ComponentValue, "value"),
            DeductHours = ReadDecimal(deductHours?.ComponentValue, "value"),
            Reason = ReadString(reason?.ComponentValue, "value")
        };
    }

    private static NaverWorksDocumentComponent? FindComponent(
        IEnumerable<NaverWorksDocumentComponent> components,
        string componentId)
    {
        return components.FirstOrDefault(component =>
            string.Equals(component.ComponentId, componentId, StringComparison.OrdinalIgnoreCase));
    }

    private static string? ReadItemName(JsonElement? value)
    {
        if (value is null ||
            !TryGetProperty(value.Value, "items", out JsonElement items) ||
            items.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        JsonElement first = items.EnumerateArray().FirstOrDefault();
        return first.ValueKind == JsonValueKind.Undefined ? null : ReadString(first, "itemName");
    }

    private static DateTime? ReadNaverDate(JsonElement? value, string propertyName)
    {
        string? text = value is null ? null : ReadString(value.Value, propertyName);
        return DateTime.TryParseExact(
            text,
            "yyyy.MM.dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateTime date)
            ? date.Date
            : null;
    }

    private static decimal? ReadDecimal(JsonElement? value, string propertyName)
    {
        string? text = value is null ? null : ReadString(value.Value, propertyName);
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal number)
            ? number
            : null;
    }

    private static string? ReadString(JsonElement? element, string propertyName)
    {
        if (element is null)
        {
            return null;
        }

        return ReadString(element.Value, propertyName);
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (!TryGetProperty(element, propertyName, out JsonElement value) ||
            value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private async Task SaveRawAsync(
        SqlConnection connection,
        NaverWorksApprovalDetailResponse detail,
        string rawJson,
        CancellationToken cancellationToken)
    {
        await using SqlCommand command = new("CDV_NaverWorksApprovalRaw_Upsert", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(SqlBigInt("@ApprovalDocumentId", detail.ApprovalDocumentId));
        command.Parameters.Add(SqlNVarChar("@DocumentFormId", 100, detail.DocumentFormId));
        command.Parameters.Add(SqlNVarChar("@DocumentNumber", 100, detail.DocumentNumber));
        command.Parameters.Add(SqlNVarChar("@UserId", 100, detail.UserId));
        command.Parameters.Add(SqlNVarChar("@UserName", 200, detail.UserName));
        command.Parameters.Add(SqlNVarChar("@OrgUnitId", 100, detail.OrgUnitId));
        command.Parameters.Add(SqlNVarChar("@OrgUnitName", 200, detail.OrgUnitName));
        command.Parameters.Add(SqlNVarChar("@Title", 500, detail.Title));
        command.Parameters.Add(SqlNVarChar("@Status", 50, detail.Status));
        command.Parameters.Add(SqlDateTimeOffset("@CreatedTime", detail.CreatedTime));
        command.Parameters.Add(SqlDateTimeOffset("@CompletedTime", detail.CompletedTime));
        command.Parameters.Add(SqlNVarChar("@RawJson", -1, rawJson));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task SaveEmployeeLeaveAsync(
        SqlConnection connection,
        NaverWorksApprovalDetailResponse detail,
        ParsedLeaveBody leave,
        CancellationToken cancellationToken)
    {
        await using SqlCommand command = new("CDV_EmployeeLeave_Upsert", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(SqlBigInt("@ApprovalDocumentId", detail.ApprovalDocumentId));
        command.Parameters.Add(SqlNVarChar("@DocumentNumber", 100, detail.DocumentNumber));
        command.Parameters.Add(SqlNVarChar("@DocumentFormId", 100, detail.DocumentFormId));
        command.Parameters.Add(SqlNVarChar("@NaverUserId", 100, detail.UserId));
        command.Parameters.Add(SqlNVarChar("@EmployeeId", 50, null));
        command.Parameters.Add(SqlNVarChar("@EmployeeName", 200, detail.UserName));
        command.Parameters.Add(SqlNVarChar("@OrgUnitId", 100, detail.OrgUnitId));
        command.Parameters.Add(SqlNVarChar("@DepartmentName", 200, detail.OrgUnitName));
        command.Parameters.Add(SqlNVarChar("@LeaveType", 100, leave.LeaveType));
        command.Parameters.Add(SqlDate("@FromDate", leave.FromDate));
        command.Parameters.Add(SqlDate("@ToDate", leave.ToDate));
        command.Parameters.Add(SqlDecimal("@LeaveDays", leave.LeaveDays));
        command.Parameters.Add(SqlNVarChar("@TimeUsed", 100, leave.TimeUsed));
        command.Parameters.Add(SqlDecimal("@DeductHours", leave.DeductHours));
        command.Parameters.Add(SqlNVarChar("@Reason", 1000, leave.Reason));
        command.Parameters.Add(SqlNVarChar("@ApprovalStatus", 50, detail.Status));
        command.Parameters.Add(SqlDateTimeOffset("@CreatedTime", detail.CreatedTime));
        command.Parameters.Add(SqlDateTimeOffset("@CompletedTime", detail.CompletedTime));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private SqlConnection CreateConnection()
    {
        string? connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
        }

        return new SqlConnection(connectionString);
    }

    private static SqlParameter SqlBigInt(string name, long value)
    {
        return new SqlParameter(name, SqlDbType.BigInt) { Value = value };
    }

    private static SqlParameter SqlNVarChar(string name, int size, string? value)
    {
        return new SqlParameter(name, SqlDbType.NVarChar, size) { Value = string.IsNullOrEmpty(value) ? DBNull.Value : value };
    }

    private static SqlParameter SqlDate(string name, DateTime? value)
    {
        return new SqlParameter(name, SqlDbType.Date) { Value = value.HasValue ? value.Value.Date : DBNull.Value };
    }

    private static SqlParameter SqlDecimal(string name, decimal? value)
    {
        SqlParameter parameter = new(name, SqlDbType.Decimal)
        {
            Precision = 18,
            Scale = 2,
            Value = value.HasValue ? value.Value : DBNull.Value
        };

        return parameter;
    }

    private static SqlParameter SqlDateTimeOffset(string name, DateTimeOffset? value)
    {
        return new SqlParameter(name, SqlDbType.DateTimeOffset) { Value = value.HasValue ? value.Value : DBNull.Value };
    }

    private sealed class ParsedLeaveBody
    {
        public string? LeaveType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? LeaveDays { get; set; }
        public string? TimeUsed { get; set; }
        public decimal? DeductHours { get; set; }
        public string? Reason { get; set; }
    }
}
