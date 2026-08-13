namespace ChangdaeVinaPurchasingApi.Dtos;

public class GoogleDriveHealthResult
{
    public bool Success { get; set; }

    public string FolderId { get; set; } = string.Empty;

    public string Account { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}
