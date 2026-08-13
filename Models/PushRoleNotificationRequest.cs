namespace ChangdaeVinaPurchasingApi.Models;

public sealed class PushRoleNotificationRequest
{
    public int RoleId { get; set; }
    public int BranchId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int RefId { get; set; }
    public string Screen { get; set; } = string.Empty;
}
