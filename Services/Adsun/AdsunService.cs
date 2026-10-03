using ChangdaeVinaPurchasingApi.Models.Adsun;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Services.Adsun;

public class AdsunService : IAdsunService
{
    private readonly HttpClient _httpClient;
    private readonly VehicleGpsLogRepository _repository;
    private readonly IOptions<AdsunOptions> _options;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdsunService> _logger;

    public AdsunService(
        HttpClient httpClient,
        VehicleGpsLogRepository repository,
        IOptions<AdsunOptions> options,
        IConfiguration configuration,
        ILogger<AdsunService> logger)
    {
        _httpClient = httpClient;
        _repository = repository;
        _options = options;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<List<Dictionary<string, object?>>> SyncGpsAsync(CancellationToken cancellationToken = default)
    {
        AdsunOptions options = _options.Value;
        string token = GetToken(options);
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("Adsun token is missing. Configure Adsun:Token or ADSUN_TOKEN.");
        }

        Uri requestUri = BuildRequestUri(options);
        using HttpRequestMessage request = new(HttpMethod.Get, requestUri);
        request.Headers.TryAddWithoutValidation("Token", token);
        request.Headers.TryAddWithoutValidation("X-Access-Token", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        string content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("ADSUN GPS sync failed. Status={StatusCode}, Body={Body}", response.StatusCode, content);
            throw new InvalidOperationException($"ADSUN request failed with status {(int)response.StatusCode}.");
        }

        AdsunDeviceStatusResponse? adsunResponse;
        try
        {
            adsunResponse = JsonSerializer.Deserialize<AdsunDeviceStatusResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid ADSUN GPS JSON response.");
            throw new InvalidOperationException("ADSUN response JSON is invalid.", ex);
        }

        List<VehicleGpsSyncItem> items = (adsunResponse?.Datas ?? [])
            .Where(item => !string.IsNullOrWhiteSpace(item.Bs) && item.TimeUpdate.HasValue)
            .Select(item => new VehicleGpsSyncItem
            {
                VehicleNo = item.Bs,
                Latitude = item.Location?.Lat,
                Longitude = item.Location?.Lng,
                Address = item.Location?.Address,
                Speed = item.Speed,
                Angle = item.Angle,
                IsEngineOn = item.TrangThaiMay,
                IsParking = item.DungDo,
                IsGpsActive = item.StatusGps,
                IsGsmLost = item.LostGsm,
                GPSTime = item.TimeUpdate
            })
            .ToList();

        string gpsJson = JsonSerializer.Serialize(items);
        return await _repository.SyncAsync(gpsJson);
    }

    private Uri BuildRequestUri(AdsunOptions options)
    {
        string baseUrl = string.IsNullOrWhiteSpace(options.BaseUrl)
            ? "https://systemroute.adsun.vn"
            : options.BaseUrl.TrimEnd('/');

        return new Uri($"{baseUrl}/api/Device/GetDeviceStatusByCompanyId?companyId={options.CompanyId}");
    }

    private string GetToken(AdsunOptions options)
    {
        return Environment.GetEnvironmentVariable("ADSUN_TOKEN")
            ?? _configuration["ADSUN_TOKEN"]
            ?? options.Token
            ?? string.Empty;
    }
}
