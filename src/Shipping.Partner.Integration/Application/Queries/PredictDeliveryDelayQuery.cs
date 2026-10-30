using Shipping.Partner.Integration.Application.Cqrs;
using Shipping.Partner.Integration.Application.Results;

namespace Shipping.Partner.Integration.Application.Queries;

public sealed record PredictDeliveryDelayQuery(float DistanceKm, float TotalWeightKg, string? ServiceType,
    int OrderHour, int DayOfWeek, int ActiveOrdersAtCreation) : IQuery<DeliveryDelayPrediction>;
