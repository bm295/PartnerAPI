namespace Shipping.Partner.Integration.Domain.Entities;

public sealed record ShippingOrder(
    Guid Id,
    Guid PartnerId,
    string OrderNumber,
    string DestinationName,
    string DestinationAddress,
    string ServiceLevel,
    decimal TotalWeightKg,
    DateTimeOffset CreatedAtUtc);
