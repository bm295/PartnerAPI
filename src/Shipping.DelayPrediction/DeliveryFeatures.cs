using Microsoft.ML.Data;

namespace Shipping.DelayPrediction;

public sealed class DeliveryFeatures
{
    [LoadColumn(0)] public float DistanceKm { get; set; }
    [LoadColumn(1)] public float TotalWeightKg { get; set; }
    [LoadColumn(2)] public string ServiceType { get; set; } = "";
    [LoadColumn(3)] public float OrderHour { get; set; }
    [LoadColumn(4)] public float DayOfWeek { get; set; }
    [LoadColumn(5)] public float ActiveOrdersAtCreation { get; set; }
    [LoadColumn(6), ColumnName("Label")] public bool IsLate { get; set; }
}

public sealed class DelayOutput
{
    [ColumnName("Probability")] public float Probability { get; set; }
}

public sealed record ModelMetadata(string ModelVersion, float DecisionThreshold, string ModelSha256);
