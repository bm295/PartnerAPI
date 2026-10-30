using Microsoft.ML;
using System.Security.Cryptography;
using System.Text.Json;

namespace Shipping.DelayPrediction;

public static class DelayModel
{
    public static ITransformer Train(MLContext ml, IDataView data) => ml.Transforms.Categorical
        .OneHotEncoding("ServiceTypeEncoded", nameof(DeliveryFeatures.ServiceType))
        .Append(ml.Transforms.Concatenate("Features", nameof(DeliveryFeatures.DistanceKm),
            nameof(DeliveryFeatures.TotalWeightKg), "ServiceTypeEncoded", nameof(DeliveryFeatures.OrderHour),
            nameof(DeliveryFeatures.DayOfWeek), nameof(DeliveryFeatures.ActiveOrdersAtCreation)))
        .Append(ml.Transforms.NormalizeMeanVariance("Features"))
        .Append(ml.BinaryClassification.Trainers.SdcaLogisticRegression(
            new Microsoft.ML.Trainers.SdcaLogisticRegressionBinaryTrainer.Options
            { LabelColumnName = "Label", FeatureColumnName = "Features", NumberOfThreads = 1 }))
        .Fit(data);

    public static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    public static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
}

// Each request gets its own PredictionEngine: engines are not thread-safe.
public sealed class DelayModelSession
{
    private readonly MLContext ml = new(seed: 42);
    private readonly ITransformer model;
    public ModelMetadata Metadata { get; }

    public DelayModelSession(string modelPath)
    {
        Metadata = JsonSerializer.Deserialize<ModelMetadata>(File.ReadAllText(modelPath + ".json"), DelayModel.JsonOptions)
            ?? throw new InvalidDataException("Missing model metadata.");
        if (string.IsNullOrWhiteSpace(Metadata.ModelVersion) || !float.IsFinite(Metadata.DecisionThreshold)
            || Metadata.DecisionThreshold <= 0 || Metadata.DecisionThreshold >= 1
            || Metadata.ModelSha256 != DelayModel.Hash(modelPath))
            throw new InvalidDataException("Invalid model metadata or model checksum mismatch.");
        model = ml.Model.Load(modelPath, out _);
        // Validate the scoring schema at load time.
        using var engine = ml.Model.CreatePredictionEngine<DeliveryFeatures, DelayOutput>(model);
    }

    public float Predict(DeliveryFeatures input)
    {
        using var engine = ml.Model.CreatePredictionEngine<DeliveryFeatures, DelayOutput>(model);
        var probability = engine.Predict(input).Probability;
        if (!float.IsFinite(probability) || probability < 0 || probability > 1)
            throw new InvalidDataException("Invalid model probability.");
        return probability;
    }
}
