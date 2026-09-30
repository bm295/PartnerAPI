namespace Shipping.Partner.Sdk;

public sealed class PartnerApiOptions
{
    public Uri? BaseUrl { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
