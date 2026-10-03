namespace ChangdaeVinaPurchasingApi.Services.Adsun;

public interface IAdsunService
{
    Task<List<Dictionary<string, object?>>> SyncGpsAsync(CancellationToken cancellationToken = default);
}
