namespace Shipping.Partner.Integration.Application;

public sealed record ShipmentEventRequest(
    Guid PartnerId,
    string TrackingNumber,
    string Status,
    string? Location,
    DateTimeOffset OccurredAtUtc);
