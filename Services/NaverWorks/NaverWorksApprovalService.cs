using ChangdaeVinaPurchasingApi.Models.NaverWorks;
using Microsoft.AspNetCore.WebUtilities;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Services.NaverWorks;

public class NaverWorksApprovalService
{
    private const string BaseUrl = "https://www.worksapis.com/v1.0";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly NaverWorksAuthService _authService;
    private readonly ILogger<NaverWorksApprovalService> _logger;

    public NaverWorksApprovalService(
        IHttpClientFactory httpClientFactory,
        NaverWorksAuthService authService,
        ILogger<NaverWorksApprovalService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _authService = authService;
        _logger = logger;
    }

    public async Task<List<NaverWorksApprovalDocument>> GetApprovalDocumentsAsync(
        DateTime fromDate,
        DateTime untilDate,
        CancellationToken cancellationToken = default)
    {
        List<NaverWorksApprovalDocument> documents = [];
        string? cursor = null;

        do
        {
            Dictionary<string, string?> query = new()
            {
                ["fromDateTime"] = FormatDateTime(fromDate),
                ["untilDateTime"] = FormatDateTime(untilDate),
                ["count"] = "50"
            };

            if (!string.IsNullOrWhiteSpace(cursor))
            {
                query["cursor"] = cursor;
            }

            string url = QueryHelpers.AddQueryString(
                $"{BaseUrl}/business-support/approval/documents",
                query);

            JsonDocument response = await SendJsonAsync(HttpMethod.Get, url, cancellationToken);
            JsonElement root = response.RootElement.Clone();
            response.Dispose();

            documents.AddRange(ReadDocuments(root));
            cursor = ReadString(root, "responseMetaData", "nextCursor");
        }
        while (!string.IsNullOrWhiteSpace(cursor));

        return documents;
    }

    public async Task<JsonElement> GetApprovalDocumentDetailJsonAsync(
        long approvalDocumentId,
        CancellationToken cancellationToken = default)
    {
        string url = $"{BaseUrl}/business-support/approval/documents/{approvalDocumentId}";
        using JsonDocument response = await SendJsonAsync(HttpMethod.Get, url, cancellationToken);
        return response.RootElement.Clone();
    }

    public NaverWorksApprovalDetailResponse ParseApprovalDetail(JsonElement root)
    {
        return new NaverWorksApprovalDetailResponse
        {
            ApprovalDocumentId = ReadLong(root, "approvalDocumentId"),
            DocumentFormId = ReadString(root, "documentFormId"),
            DocumentNumber = ReadString(root, "documentNumber"),
            Title = ReadString(root, "title"),
            Status = ReadString(root, "status"),
            UserId = ReadString(root, "userId"),
            UserName = ReadString(root, "userName"),
            OrgUnitId = ReadString(root, "orgUnitId"),
            OrgUnitName = ReadString(root, "orgUnitName"),
            CreatedTime = ReadDateTimeOffset(root, "createdTime"),
            CompletedTime = ReadDateTimeOffset(root, "completedTime"),
            DocumentBody = ReadDocumentBody(root)
        };
    }

    private async Task<JsonDocument> SendJsonAsync(
        HttpMethod method,
        string url,
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await SendAsync(method, url, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("NAVER WORKS approval API returned 401. Refreshing token once.");
            _authService.ClearAccessToken();

            using HttpResponseMessage retryResponse = await SendAsync(method, url, cancellationToken);
            return await ReadSuccessJsonAsync(retryResponse, cancellationToken);
        }

        return await ReadSuccessJsonAsync(response, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string url,
        CancellationToken cancellationToken)
    {
        string accessToken = await _authService.GetAccessTokenAsync(cancellationToken);
        HttpClient client = _httpClientFactory.CreateClient();
        using HttpRequestMessage request = new(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return await client.SendAsync(request, cancellationToken);
    }

    private static async Task<JsonDocument> ReadSuccessJsonAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"NAVER WORKS approval API returned HTTP {(int)response.StatusCode}: {responseBody}");
        }

        return JsonDocument.Parse(responseBody);
    }

    private static List<NaverWorksApprovalDocument> ReadDocuments(JsonElement root)
    {
        JsonElement array = default;

        if (!TryGetProperty(root, "documents", out array) &&
            !TryGetProperty(root, "approvalDocuments", out array) &&
            !TryGetProperty(root, "data", out array))
        {
            return [];
        }

        if (array.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        List<NaverWorksApprovalDocument> documents = [];
        foreach (JsonElement item in array.EnumerateArray())
        {
            NaverWorksApprovalDocument? document = item.Deserialize<NaverWorksApprovalDocument>(JsonOptions);
            if (document is not null)
            {
                document.ApprovalDocumentId = document.ApprovalDocumentId == 0
                    ? ReadLong(item, "approvalDocumentId")
                    : document.ApprovalDocumentId;
                document.DocumentFormId ??= ReadString(item, "documentFormId");
                documents.Add(document);
            }
        }

        return documents;
    }

    private static List<NaverWorksDocumentComponent> ReadDocumentBody(JsonElement root)
    {
        if (!TryGetProperty(root, "documentBody", out JsonElement body) ||
            body.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        List<NaverWorksDocumentComponent> components = [];
        foreach (JsonElement item in body.EnumerateArray())
        {
            string? componentId = ReadString(item, "componentId");
            if (string.IsNullOrWhiteSpace(componentId))
            {
                continue;
            }

            TryGetProperty(item, "componentValue", out JsonElement componentValue);
            components.Add(new NaverWorksDocumentComponent
            {
                ComponentId = componentId,
                ComponentValue = componentValue.Clone()
            });
        }

        return components;
    }

    private static string FormatDateTime(DateTime value)
    {
        return value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
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

    private static string? ReadString(JsonElement element, string name)
    {
        if (!TryGetProperty(element, name, out JsonElement value) ||
            value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static string? ReadString(JsonElement element, string objectName, string name)
    {
        return TryGetProperty(element, objectName, out JsonElement parent)
            ? ReadString(parent, name)
            : null;
    }

    private static long ReadLong(JsonElement element, string name)
    {
        if (!TryGetProperty(element, name, out JsonElement value))
        {
            return 0;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out long number))
        {
            return number;
        }

        return value.ValueKind == JsonValueKind.String &&
            long.TryParse(value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out number)
            ? number
            : 0;
    }

    private static DateTimeOffset? ReadDateTimeOffset(JsonElement element, string name)
    {
        string? value = ReadString(element, name);
        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset dateTime)
            ? dateTime
            : null;
    }
}
