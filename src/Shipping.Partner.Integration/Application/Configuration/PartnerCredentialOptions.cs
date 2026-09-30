namespace Shipping.Partner.Integration.Application.Configuration;

public sealed class PartnerCredentialOptions
{
    public const string SectionName = "PartnerCredentials";

    public int SecretLength { get; init; } = 64;
    public TimeSpan ExpirationPeriod { get; init; } = TimeSpan.FromDays(365);
    public TimeSpan AllowedClockSkew { get; init; } = TimeSpan.FromMinutes(5);
    public string HeaderName { get; init; } = "X-Shipping-Partner-Key";
}
