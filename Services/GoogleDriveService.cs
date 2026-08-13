using ChangdaeVinaPurchasingApi.Contracts;
using ChangdaeVinaPurchasingApi.Dtos;
using ChangdaeVinaPurchasingApi.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Microsoft.Extensions.Options;

namespace ChangdaeVinaPurchasingApi.Services;

public class GoogleDriveService : IGoogleDriveService
{
    private const string ApplicationName = "MrSol";
    private const string OAuthUserId = "mrsol";
    private const long MaxFileSizeBytes = 20 * 1024 * 1024;
    private readonly ILogger<GoogleDriveService> _logger;
    private readonly GoogleDriveOptions _options;

    public GoogleDriveService(
        IOptions<GoogleDriveOptions> options,
        ILogger<GoogleDriveService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Validates Google Drive OAuth configuration, refresh token, and folder access.
    /// </summary>
    public async Task<GoogleDriveHealthResult> CheckHealthAsync()
    {
        try
        {
            ValidateOAuthOptions();
            string folderId = GetFolderId();
            UserCredential credential = CreateOAuthCredential();
            await RefreshAccessTokenAsync(credential);
            DriveService driveService = CreateDriveService(credential);

            FilesResource.GetRequest request = driveService.Files.Get(folderId);
            request.Fields = "id,name,mimeType";
            request.SupportsAllDrives = true;

            Google.Apis.Drive.v3.Data.File folder = await request.ExecuteAsync();
            if (!string.Equals(folder.MimeType, "application/vnd.google-apps.folder", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("GoogleDrive:FolderId does not point to a Google Drive folder.");
            }

            return new GoogleDriveHealthResult
            {
                Success = true,
                FolderId = folderId,
                Account = "Google OAuth User",
                Message = "Google Drive OAuth configuration is valid."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google Drive health check failed.");
            throw;
        }
    }

    /// <summary>
    /// Uploads a file to the configured Google Drive folder and returns its public metadata.
    /// </summary>
    public async Task<GoogleUploadResult> UploadAsync(IFormFile file)
    {
        if (file.Length > MaxFileSizeBytes)
        {
            throw new ArgumentException("File size must be less than or equal to 20 MB.", nameof(file));
        }

        try
        {
            _logger.LogInformation("[GoogleDrive] Uploading file: {FileName}", file.FileName);
            DriveService driveService = CreateDriveService(CreateOAuthCredential());
            Google.Apis.Drive.v3.Data.File fileMetadata = new()
            {
                Name = file.FileName,
                Parents = [GetFolderId()]
            };

            await using Stream fileStream = file.OpenReadStream();
            FilesResource.CreateMediaUpload request = driveService.Files.Create(
                fileMetadata,
                fileStream,
                string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);

            request.Fields = "id,name,size";
            request.SupportsAllDrives = true;
            IUploadProgress uploadProgress = await request.UploadAsync();
            if (uploadProgress.Status == UploadStatus.Failed)
            {
                throw uploadProgress.Exception ?? new InvalidOperationException("Google Drive upload failed.");
            }

            Google.Apis.Drive.v3.Data.File uploadedFile = request.ResponseBody
                ?? throw new InvalidOperationException("Google Drive did not return uploaded file metadata.");

            string fileUrl = await MakePublicAsync(uploadedFile.Id);
            _logger.LogInformation("[GoogleDrive] File uploaded successfully. FileId={FileId}", uploadedFile.Id);

            return new GoogleUploadResult
            {
                FileId = uploadedFile.Id,
                FileName = uploadedFile.Name ?? file.FileName,
                FileUrl = fileUrl,
                FileSize = uploadedFile.Size ?? file.Length
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file {FileName} to Google Drive.", file.FileName);
            throw;
        }
    }

    /// <summary>
    /// Downloads a Google Drive file as a stream.
    /// </summary>
    public async Task<Stream> DownloadAsync(string fileId)
    {
        try
        {
            DriveService driveService = CreateDriveService(CreateOAuthCredential());
            MemoryStream stream = new();
            await driveService.Files.Get(fileId).DownloadAsync(stream);
            stream.Position = 0;
            return stream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download Google Drive file {FileId}.", fileId);
            throw;
        }
    }

    /// <summary>
    /// Deletes a file from Google Drive.
    /// </summary>
    public async Task DeleteAsync(string fileId)
    {
        try
        {
            _logger.LogInformation("[GoogleDrive] Deleting file: {FileId}", fileId);
            DriveService driveService = CreateDriveService(CreateOAuthCredential());
            await driveService.Files.Delete(fileId).ExecuteAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete Google Drive file {FileId}.", fileId);
            throw;
        }
    }

    /// <summary>
    /// Grants public reader permission to a Google Drive file and returns its public URL.
    /// </summary>
    public async Task<string> MakePublicAsync(string fileId)
    {
        try
        {
            DriveService driveService = CreateDriveService(CreateOAuthCredential());
            Permission permission = new()
            {
                Type = "anyone",
                Role = "reader"
            };

            PermissionsResource.CreateRequest request = driveService.Permissions.Create(permission, fileId);
            request.Fields = "id";
            request.SupportsAllDrives = true;
            await request.ExecuteAsync();

            return CreatePublicUrl(fileId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to make Google Drive file {FileId} public.", fileId);
            throw;
        }
    }

    private UserCredential CreateOAuthCredential()
    {
        ValidateOAuthOptions();
        _logger.LogInformation("[GoogleDrive] Initializing OAuth credential...");

        TokenResponse token = new()
        {
            RefreshToken = _options.RefreshToken
        };

        GoogleAuthorizationCodeFlow flow = new(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret
            },
            Scopes = [DriveService.Scope.Drive]
        });

        return new UserCredential(flow, OAuthUserId, token);
    }

    private async Task RefreshAccessTokenAsync(UserCredential credential)
    {
        bool refreshed = await credential.RefreshTokenAsync(CancellationToken.None);
        if (!refreshed)
        {
            throw new InvalidOperationException("Invalid RefreshToken.");
        }

        _logger.LogInformation("[GoogleDrive] Access token refreshed successfully.");
    }

    private DriveService CreateDriveService(UserCredential credential)
    {
        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName
        });
    }

    private void ValidateOAuthOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId) ||
            string.Equals(_options.ClientId, "REPLACE_CLIENT_ID", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Missing ClientId.");
        }

        if (string.IsNullOrWhiteSpace(_options.ClientSecret) ||
            string.Equals(_options.ClientSecret, "REPLACE_CLIENT_SECRET", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Missing ClientSecret.");
        }

        if (string.IsNullOrWhiteSpace(_options.RefreshToken) ||
            string.Equals(_options.RefreshToken, "REPLACE_REFRESH_TOKEN", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Missing RefreshToken.");
        }
    }

    private string GetFolderId()
    {
        if (string.IsNullOrWhiteSpace(_options.FolderId) ||
            string.Equals(_options.FolderId, "REPLACE_FOLDER_ID", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("GoogleDrive:FolderId is not configured.");
        }

        return _options.FolderId;
    }

    private static string CreatePublicUrl(string fileId)
    {
        return $"https://drive.google.com/uc?id={Uri.EscapeDataString(fileId)}";
    }
}
