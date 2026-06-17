namespace Shipping.Partner.Integration.Domain;

public sealed record ShippingPartnerConnection(
    Guid Id,
    string Name,
    string ExternalReference,
    string ApiKey,
    DateTimeOffset ConnectedAtUtc);
