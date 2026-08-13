using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChangdaeVinaPurchasingApi.Models.NaverWorks;

public class NaverWorksTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public JsonElement ExpiresIn { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}

public class NaverWorksApprovalDocument
{
    public long ApprovalDocumentId { get; set; }
    public string? DocumentFormId { get; set; }
    public string? DocumentNumber { get; set; }
    public string? Title { get; set; }
    public string? Status { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? OrgUnitId { get; set; }
    public string? OrgUnitName { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }
    public DateTimeOffset? CompletedTime { get; set; }
}

public class NaverWorksApprovalDetailResponse : NaverWorksApprovalDocument
{
    public List<NaverWorksDocumentComponent> DocumentBody { get; set; } = [];
}

public class NaverWorksDocumentComponent
{
    public string? ComponentId { get; set; }
    public JsonElement ComponentValue { get; set; }
}

public class NaverWorksSyncResult
{
    public bool Success => Failed == 0;
    public int TotalDocuments { get; set; }
    public int LeaveDocuments { get; set; }
    public int Synced { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = [];
}

public class SyncLeaveRequest
{
    public DateTime FromDate { get; set; }
    public DateTime UntilDate { get; set; }
}
