using Shipping.Partner.Integration.Domain.Enums;

namespace Shipping.Partner.Integration.Domain.Entities;

public sealed record ShipmentEventRecord(
    Guid Id,
    Guid PartnerId,
    string TrackingNumber,
    ShipmentStatus Status,
    string? Location,
    DateTimeOffset OccurredAtUtc,
    DateTimeOffset ReceivedAtUtc);
