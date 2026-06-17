namespace Shipping.Partner.Integration.Domain;

public sealed record ShipmentEventRecord(
    Guid Id,
    Guid PartnerId,
    string TrackingNumber,
    string Status,
    string? Location,
    DateTimeOffset OccurredAtUtc,
    DateTimeOffset ReceivedAtUtc);
