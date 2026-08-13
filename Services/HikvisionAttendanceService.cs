using System.Net;
using System.Text;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Services;

public class HikvisionAttendanceService
{
    private readonly IConfiguration _configuration;

    public HikvisionAttendanceService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<JsonElement> GetTodayCheckinsAsync(CancellationToken cancellationToken = default)
    {
        string url = GetRequiredSetting("Url");
        string username = GetRequiredSetting("Username");
        string password = GetRequiredSetting("Password");

        DateTimeOffset now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));
        DateTimeOffset startTime = new(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        DateTimeOffset endTime = new(now.Year, now.Month, now.Day, 23, 59, 59, now.Offset);

        object requestBody = new
        {
            AcsEventCond = new
            {
                searchID = "checkin-001",
                searchResultPosition = 0,
                maxResults = 50,
                major = 5,
                minor = 0,
                startTime = startTime.ToString("yyyy-MM-dd'T'HH:mm:sszzz"),
                endTime = endTime.ToString("yyyy-MM-dd'T'HH:mm:sszzz")
            }
        };

        using HttpClientHandler handler = new()
        {
            Credentials = new NetworkCredential(username, password),
            PreAuthenticate = true
        };
        using HttpClient client = new(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        using HttpRequestMessage request = new(HttpMethod.Post, url)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json")
        };
        using HttpResponseMessage response = await client.SendAsync(request, cancellationToken);
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Máy chấm công trả về HTTP {(int)response.StatusCode} ({response.ReasonPhrase}): {responseBody}");
        }

        return JsonSerializer.Deserialize<JsonElement>(responseBody);
    }

    private string GetRequiredSetting(string name)
    {
        string key = $"HikvisionAttendance:{name}";
        string? value = _configuration[key];

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Chưa cấu hình '{key}'.");
        }

        return value;
    }
}
