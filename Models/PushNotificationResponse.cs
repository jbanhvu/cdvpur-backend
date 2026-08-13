namespace ChangdaeVinaPurchasingApi.Models;

using System.Text.Json.Serialization;

public sealed class PushNotificationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? SuccessCount { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? FailureCount { get; set; }

    public static PushNotificationResponse Ok(string message, int successCount, int failureCount)
    {
        return new PushNotificationResponse
        {
            Success = true,
            Message = message,
            SuccessCount = successCount,
            FailureCount = failureCount
        };
    }

    public static PushNotificationResponse Fail(string message, int successCount = 0, int failureCount = 0)
    {
        return new PushNotificationResponse
        {
            Success = false,
            Message = message,
            SuccessCount = successCount == 0 ? null : successCount,
            FailureCount = failureCount == 0 ? null : failureCount
        };
    }
}
