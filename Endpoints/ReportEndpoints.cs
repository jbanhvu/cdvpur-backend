using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/reports")
            .WithTags("Reports");

        group.MapPost("inventory", async (InventoryReportRepository repository, Dictionary<string, JsonElement> body) =>
        {
            int year = GetRequiredInt(body, "Year");
            int month = GetRequiredInt(body, "Month");

            if (month < 1 || month > 12)
            {
                throw new ArgumentException("Field 'month' must be between 1 and 12.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.GetInventoryReportAsync(year, month));
        });

        group.MapPost("employee-leave-summary", async (EmployeeLeaveRepository repository, Dictionary<string, JsonElement> body) =>
        {
            DateTime fromDate = GetRequiredDate(body, "FromDate");
            DateTime toDate = GetRequiredDate(body, "ToDate");
            string? departmentName = GetOptionalString(body, "DepartmentName");

            if (fromDate.Date > toDate.Date)
            {
                throw new ArgumentException("Field 'fromDate' must be less than or equal to 'toDate'.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.GetSummaryAsync(fromDate, toDate, departmentName));
        });

        group.MapPost("employee-leaves", async (EmployeeLeaveRepository repository, Dictionary<string, JsonElement> body) =>
        {
            DateTime? fromDate = GetOptionalDate(body, "FromDate");
            DateTime? toDate = GetOptionalDate(body, "ToDate");
            string? approvalStatus = GetOptionalString(body, "ApprovalStatus");

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
            {
                throw new ArgumentException("Field 'fromDate' must be less than or equal to 'toDate'.");
            }

            return await ApiResponseHelper.HandleAsync(() => repository.SelectAsync(fromDate, toDate, approvalStatus));
        });

        return app;
    }

    private static int GetRequiredInt(IReadOnlyDictionary<string, JsonElement> body, string name)
    {
        foreach (KeyValuePair<string, JsonElement> item in body)
        {
            if (!string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (item.Value.ValueKind == JsonValueKind.Number && item.Value.TryGetInt32(out int number))
            {
                return number;
            }

            if (item.Value.ValueKind == JsonValueKind.String && int.TryParse(item.Value.GetString(), out number))
            {
                return number;
            }

            break;
        }

        throw new ArgumentException($"Field '{name}' must be an integer.");
    }

    private static DateTime GetRequiredDate(IReadOnlyDictionary<string, JsonElement> body, string name)
    {
        foreach (KeyValuePair<string, JsonElement> item in body)
        {
            if (!string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string value = item.Value.ValueKind == JsonValueKind.String
                ? item.Value.GetString() ?? string.Empty
                : item.Value.ToString();

            if (DateTime.TryParse(value, out DateTime date))
            {
                return date.Date;
            }

            break;
        }

        throw new ArgumentException($"Field '{name}' must be a date.");
    }

    private static DateTime? GetOptionalDate(IReadOnlyDictionary<string, JsonElement> body, string name)
    {
        foreach (KeyValuePair<string, JsonElement> item in body)
        {
            if (!string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (item.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                return null;
            }

            string value = item.Value.ValueKind == JsonValueKind.String
                ? item.Value.GetString() ?? string.Empty
                : item.Value.ToString();

            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (DateTime.TryParse(value, out DateTime date))
            {
                return date.Date;
            }

            break;
        }

        return null;
    }

    private static string? GetOptionalString(IReadOnlyDictionary<string, JsonElement> body, string name)
    {
        foreach (KeyValuePair<string, JsonElement> item in body)
        {
            if (!string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (item.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                return null;
            }

            string value = item.Value.ValueKind == JsonValueKind.String
                ? item.Value.GetString() ?? string.Empty
                : item.Value.ToString();

            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        return null;
    }
}
