namespace Shipping.Partner.Integration.Application.Configuration;

public sealed record ShippingPartnerIntegrationOptions
{
    public const string SectionName = "ShippingPartnerIntegration";

    public string ApiKeyHeaderName { get; init; } = "X-Shipping-Partner-Key";
    public Guid DefaultPartnerId { get; init; }
}
