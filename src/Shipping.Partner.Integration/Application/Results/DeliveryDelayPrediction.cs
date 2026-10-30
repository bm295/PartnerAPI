namespace Shipping.Partner.Integration.Application.Results;

public sealed record DeliveryDelayPrediction(bool PredictedLate, float DelayProbability, float DecisionThreshold, string ModelVersion);
