using System.Text.Json.Serialization;

namespace Shipping.Partner.Integration.Api.Contracts;

public sealed record PredictDeliveryDelayRequest(
    [property: JsonRequired] float DistanceKm,
    [property: JsonRequired] float TotalWeightKg,
    [property: JsonRequired] string? ServiceType,
    [property: JsonRequired] int OrderHour,
    [property: JsonRequired] int DayOfWeek,
    [property: JsonRequired] int ActiveOrdersAtCreation);
