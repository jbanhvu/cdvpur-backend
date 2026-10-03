namespace ChangdaeVinaPurchasingApi.Models.Adsun;

public class AdsunOptions
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = "https://systemroute.adsun.vn";
    public int CompanyId { get; set; } = 2172;
    public string Token { get; set; } = string.Empty;
    public int SyncIntervalSeconds { get; set; } = 60;
}
