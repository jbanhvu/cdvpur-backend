using ChangdaeVinaPurchasingApi.Dtos;

namespace ChangdaeVinaPurchasingApi.Contracts;

public interface IGoogleDriveService
{
    /// <summary>
    /// Validates Google Drive OAuth configuration, refresh token, and folder access.
    /// </summary>
    Task<GoogleDriveHealthResult> CheckHealthAsync();

    /// <summary>
    /// Uploads a file to the configured Google Drive folder and returns its public metadata.
    /// </summary>
    Task<GoogleUploadResult> UploadAsync(IFormFile file);

    /// <summary>
    /// Downloads a Google Drive file as a stream.
    /// </summary>
    Task<Stream> DownloadAsync(string fileId);

    /// <summary>
    /// Deletes a file from Google Drive.
    /// </summary>
    Task DeleteAsync(string fileId);

    /// <summary>
    /// Grants public reader permission to a Google Drive file and returns its public URL.
    /// </summary>
    Task<string> MakePublicAsync(string fileId);
}
