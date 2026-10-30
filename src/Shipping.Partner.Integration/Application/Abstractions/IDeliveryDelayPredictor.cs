using Shipping.Partner.Integration.Application.Queries;
using Shipping.Partner.Integration.Application.Results;

namespace Shipping.Partner.Integration.Application.Abstractions;

public interface IDeliveryDelayPredictor
{
    DeliveryDelayPrediction Predict(PredictDeliveryDelayQuery input);
}

public sealed class PredictionUnavailableException : Exception
{
    public PredictionUnavailableException() : base("Delivery delay model is not available. Train and configure a model first.") { }
}
