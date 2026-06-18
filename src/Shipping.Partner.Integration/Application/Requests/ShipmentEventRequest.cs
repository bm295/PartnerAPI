namespace Shipping.Partner.Integration.Application.Requests;

public sealed record ShipmentEventRequest(
    Guid PartnerId,
    string TrackingNumber,
    string Status,
    string? Location,
    DateTimeOffset OccurredAtUtc);
