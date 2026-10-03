using System.Text.Json.Serialization;

namespace ChangdaeVinaPurchasingApi.Models.Adsun;

public class AdsunDeviceStatusResponse
{
    [JsonPropertyName("Datas")]
    public List<AdsunDeviceStatus> Datas { get; set; } = [];
}

public class AdsunDeviceStatus
{
    public string? Serial { get; set; }
    public string? Id { get; set; }
    public string? Bs { get; set; }
    public AdsunLocation? Location { get; set; }

    [JsonPropertyName("speed")]
    public decimal? Speed { get; set; }

    public int? Angle { get; set; }

    [JsonPropertyName("statusGps")]
    public bool? StatusGps { get; set; }

    [JsonPropertyName("lostgsm")]
    public bool? LostGsm { get; set; }

    [JsonPropertyName("trangThaiMay")]
    public bool? TrangThaiMay { get; set; }

    [JsonPropertyName("dungDo")]
    public bool? DungDo { get; set; }

    [JsonPropertyName("timeUpdate")]
    public DateTime? TimeUpdate { get; set; }
}

public class AdsunLocation
{
    public decimal? Lat { get; set; }
    public decimal? Lng { get; set; }
    public string? Address { get; set; }
}

public class VehicleGpsSyncItem
{
    public string? VehicleNo { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Address { get; set; }
    public decimal? Speed { get; set; }
    public int? Angle { get; set; }
    public bool? IsEngineOn { get; set; }
    public bool? IsParking { get; set; }
    public bool? IsGpsActive { get; set; }
    public bool? IsGsmLost { get; set; }
    public DateTime? GPSTime { get; set; }
}
