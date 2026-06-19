namespace Shipping.Partner.Integration.Domain.Entities;

public sealed record ShippingPartnerConnection(
    Guid Id,
    string Name,
    string ExternalReference,
    string ApiKey,
    DateTimeOffset ConnectedAtUtc);
