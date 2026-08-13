using ChangdaeVinaPurchasingApi.Models.NaverWorks;
using ChangdaeVinaPurchasingApi.Services.NaverWorks;
using Microsoft.AspNetCore.Mvc;

namespace ChangdaeVinaPurchasingApi.Controllers;

[ApiController]
[Route("api/naverworks")]
public class NaverWorksController : ControllerBase
{
    private readonly NaverWorksSyncService _syncService;
    private readonly ILogger<NaverWorksController> _logger;

    public NaverWorksController(
        NaverWorksSyncService syncService,
        ILogger<NaverWorksController> logger)
    {
        _syncService = syncService;
        _logger = logger;
    }

    [HttpPost("sync-leave")]
    public async Task<IActionResult> SyncLeave([FromBody] SyncLeaveRequest request, CancellationToken cancellationToken)
    {
        if (request.FromDate == default || request.UntilDate == default)
        {
            return BadRequest(new { success = false, message = "fromDate and untilDate are required." });
        }

        if (request.FromDate.Date > request.UntilDate.Date)
        {
            return BadRequest(new { success = false, message = "fromDate must be less than or equal to untilDate." });
        }

        if (request.UntilDate.Date > request.FromDate.Date.AddMonths(1))
        {
            return BadRequest(new { success = false, message = "Date range must be less than or equal to 1 month." });
        }

        try
        {
            DateTime fromDate = request.FromDate.Date;
            DateTime untilDate = request.UntilDate.Date.AddDays(1).AddTicks(-1);
            NaverWorksSyncResult result = await _syncService.SyncLeaveAsync(fromDate, untilDate, cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "NAVER WORKS leave sync failed.");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "NAVER WORKS API request failed.");
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                success = false,
                message = "NAVER WORKS API request failed.",
                detail = ex.Message
            });
        }
    }
}
