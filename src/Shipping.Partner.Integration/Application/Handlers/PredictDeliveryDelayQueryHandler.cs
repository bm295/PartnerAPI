using Shipping.Partner.Integration.Application.Abstractions;
using Shipping.Partner.Integration.Application.Cqrs;
using Shipping.Partner.Integration.Application.Queries;
using Shipping.Partner.Integration.Application.Results;

namespace Shipping.Partner.Integration.Application.Handlers;

public sealed class PredictDeliveryDelayQueryHandler(IDeliveryDelayPredictor predictor)
    : IQueryHandler<PredictDeliveryDelayQuery, DeliveryDelayPrediction>
{
    public DeliveryDelayPrediction Handle(PredictDeliveryDelayQuery query)
    {
        if (!float.IsFinite(query.DistanceKm) || query.DistanceKm <= 0 || query.DistanceKm > 500
            || !float.IsFinite(query.TotalWeightKg) || query.TotalWeightKg <= 0 || query.TotalWeightKg > 1000
            || query.ServiceType is not ("standard" or "express")
            || query.OrderHour is < 0 or > 23 || query.DayOfWeek is < 0 or > 6
            || query.ActiveOrdersAtCreation is < 0 or > 10000)
            throw new ArgumentException("Expected distance (0,500], weight (0,1000], serviceType standard/express, hour 0..23, day 0..6, active orders 0..10000.");
        return predictor.Predict(query);
    }
}
