using ChangdaeVinaPurchasingApi.Contracts;
using Google;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ChangdaeVinaPurchasingApi.Controllers;

[ApiController]
[Route("api/google-drive")]
public class GoogleDriveController : ControllerBase
{
    private const long MaxFileSizeBytes = 20 * 1024 * 1024;
    private readonly IGoogleDriveService _googleDriveService;
    private readonly ILogger<GoogleDriveController> _logger;

    public GoogleDriveController(
        IGoogleDriveService googleDriveService,
        ILogger<GoogleDriveController> logger)
    {
        _googleDriveService = googleDriveService;
        _logger = logger;
    }

    /// <summary>
    /// Checks Google Drive OAuth configuration, refresh token, API connectivity, and folder access.
    /// </summary>
    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        try
        {
            return Ok(await _googleDriveService.CheckHealthAsync());
        }
        catch (TokenResponseException ex)
        {
            _logger.LogError(ex, "Google Drive OAuth refresh token is invalid.");
            return StatusCode(StatusCodes.Status401Unauthorized, new
            {
                success = false,
                message = "Invalid RefreshToken.",
                detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Google Drive configuration is invalid.");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogError(ex, "Google Drive folder was not found or the OAuth user does not have access.");
            return NotFound(new
            {
                success = false,
                message = "Folder not found.",
                detail = ex.Message
            });
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            _logger.LogError(ex, "Google Drive OAuth user does not have access to the configured folder or Drive API is disabled.");
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                success = false,
                message = "Google Drive API disabled or OAuth user does not have access to the configured folder.",
                detail = ex.Message
            });
        }
        catch (GoogleApiException ex)
        {
            _logger.LogError(ex, "Google Drive API health check failed.");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = "Google Drive API health check failed.",
                detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected Google Drive health check error.");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = "Unexpected Google Drive health check error.",
                detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Uploads a multipart form-data file to Google Drive.
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "File is required." });
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return BadRequest(new { message = "File size must be less than or equal to 20 MB." });
        }

        try
        {
            return Ok(await _googleDriveService.UploadAsync(file));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid Google Drive upload request.");
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Google Drive upload is not configured correctly.");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (TokenResponseException ex)
        {
            _logger.LogError(ex, "Google Drive OAuth refresh token is invalid.");
            return StatusCode(StatusCodes.Status401Unauthorized, new
            {
                success = false,
                message = "Invalid RefreshToken.",
                detail = ex.Message
            });
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogError(ex, "Google Drive folder was not found or the OAuth user does not have access.");
            return NotFound(new
            {
                success = false,
                message = "Folder not found.",
                detail = ex.Message
            });
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.Forbidden)
        {
            _logger.LogError(ex, "Google Drive upload was forbidden.");
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                success = false,
                message = "Google Drive upload was forbidden. Check folder sharing and public sharing policy.",
                detail = ex.Message
            });
        }
        catch (GoogleApiException ex)
        {
            _logger.LogError(ex, "Google Drive API upload failed.");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = "Google Drive API upload failed.",
                detail = ex.Message
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Google Drive upload could not reach Google APIs.");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = "Google Drive upload could not reach Google APIs.",
                detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected Google Drive upload error.");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = "Failed to upload file to Google Drive.",
                detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Downloads a Google Drive file and streams it to the client.
    /// </summary>
    [HttpGet("download/{fileId}")]
    public async Task<IActionResult> Download(string fileId)
    {
        if (string.IsNullOrWhiteSpace(fileId))
        {
            return BadRequest(new { message = "FileId is required." });
        }

        try
        {
            Stream stream = await _googleDriveService.DownloadAsync(fileId);
            return File(stream, "application/octet-stream");
        }
        catch (TokenResponseException ex)
        {
            _logger.LogError(ex, "Google Drive OAuth refresh token is invalid.");
            return StatusCode(StatusCodes.Status401Unauthorized, new
            {
                success = false,
                message = "Invalid RefreshToken.",
                detail = ex.Message
            });
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning(ex, "Google Drive file {FileId} was not found.", fileId);
            return NotFound(new { message = "File was not found." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected Google Drive download error for file {FileId}.", fileId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to download file from Google Drive." });
        }
    }

    /// <summary>
    /// Deletes a Google Drive file.
    /// </summary>
    [HttpDelete("{fileId}")]
    public async Task<IActionResult> Delete(string fileId)
    {
        if (string.IsNullOrWhiteSpace(fileId))
        {
            return BadRequest(new { message = "FileId is required." });
        }

        try
        {
            await _googleDriveService.DeleteAsync(fileId);
            return Ok(new { success = true });
        }
        catch (TokenResponseException ex)
        {
            _logger.LogError(ex, "Google Drive OAuth refresh token is invalid.");
            return StatusCode(StatusCodes.Status401Unauthorized, new
            {
                success = false,
                message = "Invalid RefreshToken.",
                detail = ex.Message
            });
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning(ex, "Google Drive file {FileId} was not found.", fileId);
            return NotFound(new { message = "File was not found." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected Google Drive delete error for file {FileId}.", fileId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to delete file from Google Drive." });
        }
    }
}
