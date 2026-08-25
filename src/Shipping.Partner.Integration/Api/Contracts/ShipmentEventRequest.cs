using Shipping.Partner.Integration.Domain.Enums;

namespace Shipping.Partner.Integration.Api.Contracts;

public sealed record ShipmentEventRequest(
    Guid PartnerId,
    string TrackingNumber,
    ShipmentStatus Status,
    string? Location,
    DateTimeOffset OccurredAtUtc);
