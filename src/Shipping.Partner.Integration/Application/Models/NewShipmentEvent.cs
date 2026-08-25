using Shipping.Partner.Integration.Domain.Enums;

namespace Shipping.Partner.Integration.Application.Models;

public sealed record NewShipmentEvent(
    Guid PartnerId,
    string TrackingNumber,
    ShipmentStatus Status,
    string? Location,
    DateTimeOffset OccurredAtUtc);
