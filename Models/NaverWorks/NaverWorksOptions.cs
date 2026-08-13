namespace ChangdaeVinaPurchasingApi.Models.NaverWorks;

public class NaverWorksOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string ServiceAccount { get; set; } = string.Empty;
    public string PrivateKeyPath { get; set; } = "Keys/naverworks-private.key";
    public string Scope { get; set; } = "businessSupport.approval.read";
    public string LeaveDocumentFormId { get; set; } = string.Empty;
}
