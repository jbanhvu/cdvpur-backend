using ChangdaeVinaPurchasingApi.Models.Adsun;
using Microsoft.Extensions.Options;

namespace ChangdaeVinaPurchasingApi.Services.Adsun;

public class AdsunGpsSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<AdsunOptions> _options;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdsunGpsSyncBackgroundService> _logger;

    public AdsunGpsSyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<AdsunOptions> options,
        IConfiguration configuration,
        ILogger<AdsunGpsSyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        AdsunOptions options = _options.Value;
        if (!options.Enabled)
        {
            _logger.LogInformation("ADSUN GPS background sync is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(GetToken(options)))
        {
            _logger.LogWarning("ADSUN GPS background sync is enabled but token is missing.");
            return;
        }

        int intervalSeconds = Math.Max(options.SyncIntervalSeconds, 10);
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(intervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                IAdsunService service = scope.ServiceProvider.GetRequiredService<IAdsunService>();
                await service.SyncGpsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ADSUN GPS background sync failed.");
            }

            try
            {
                if (!await timer.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private string GetToken(AdsunOptions options)
    {
        return Environment.GetEnvironmentVariable("ADSUN_TOKEN")
            ?? _configuration["ADSUN_TOKEN"]
            ?? options.Token
            ?? string.Empty;
    }
}
