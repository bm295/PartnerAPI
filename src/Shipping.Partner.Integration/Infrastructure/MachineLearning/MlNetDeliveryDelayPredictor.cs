using Shipping.DelayPrediction;
using Shipping.Partner.Integration.Application.Abstractions;
using Shipping.Partner.Integration.Application.Queries;
using Shipping.Partner.Integration.Application.Results;

namespace Shipping.Partner.Integration.Infrastructure.MachineLearning;

public sealed class MlNetDeliveryDelayPredictor : IDeliveryDelayPredictor
{
    private readonly DelayModelSession? session;
    public MlNetDeliveryDelayPredictor(string path, ILogger<MlNetDeliveryDelayPredictor> logger)
    {
        try { session = new DelayModelSession(path); }
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException or ArgumentException
            or InvalidOperationException or System.Text.Json.JsonException)
        {
            logger.LogWarning(ex, "Delivery delay model unavailable at {ModelPath}", path);
        }
    }
    public DeliveryDelayPrediction Predict(PredictDeliveryDelayQuery input)
    {
        if (session is null) throw new PredictionUnavailableException();
        var probability = session.Predict(new DeliveryFeatures { DistanceKm = input.DistanceKm,
            TotalWeightKg = input.TotalWeightKg, ServiceType = input.ServiceType!, OrderHour = input.OrderHour,
            DayOfWeek = input.DayOfWeek, ActiveOrdersAtCreation = input.ActiveOrdersAtCreation });
        return new(probability >= session.Metadata.DecisionThreshold, probability,
            session.Metadata.DecisionThreshold, session.Metadata.ModelVersion);
    }
}
