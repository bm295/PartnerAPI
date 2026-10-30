using Microsoft.ML;
using Shipping.DelayPrediction;
using System.Text.Json;

if (args.Length != 3)
{
    Console.Error.WriteLine("Usage: trainer <chronologically-sorted.csv> <model.zip> <version>");
    return 1;
}
try
{
    var ml = new MLContext(seed: 42);
    if (string.IsNullOrWhiteSpace(args[2])) throw new ArgumentException("Model version must not be empty.");
    var lines = File.ReadAllLines(args[0]);
    if (lines.Length == 0 || lines[0] != "DistanceKm,TotalWeightKg,ServiceType,OrderHour,DayOfWeek,ActiveOrdersAtCreation,IsLate")
        throw new InvalidDataException("Unexpected CSV header.");
    var rows = lines.Skip(1).Select((line, index) =>
    {
        var values = line.Split(',');
        if (values.Length != 7 || values.Any(string.IsNullOrWhiteSpace))
            throw new InvalidDataException($"Missing or extra value on CSV line {index + 2}.");
        float Number(int i) => float.Parse(values[i], System.Globalization.CultureInfo.InvariantCulture);
        return new DeliveryFeatures { DistanceKm = Number(0), TotalWeightKg = Number(1), ServiceType = values[2],
            OrderHour = Number(3), DayOfWeek = Number(4), ActiveOrdersAtCreation = Number(5), IsLate = bool.Parse(values[6]) };
    }).ToArray();
    if (rows.Length < 100) throw new InvalidDataException("At least 100 ordered observations are required.");
    foreach (var row in rows)
    {
        if (!float.IsFinite(row.DistanceKm) || row.DistanceKm <= 0 || row.DistanceKm > 500
            || !float.IsFinite(row.TotalWeightKg) || row.TotalWeightKg <= 0 || row.TotalWeightKg > 1000
            || row.ServiceType is not ("standard" or "express")
            || !float.IsFinite(row.OrderHour) || row.OrderHour < 0 || row.OrderHour > 23 || row.OrderHour % 1 != 0
            || !float.IsFinite(row.DayOfWeek) || row.DayOfWeek < 0 || row.DayOfWeek > 6 || row.DayOfWeek % 1 != 0
            || !float.IsFinite(row.ActiveOrdersAtCreation) || row.ActiveOrdersAtCreation < 0
            || row.ActiveOrdersAtCreation > 10000 || row.ActiveOrdersAtCreation % 1 != 0)
            throw new InvalidDataException("Invalid or missing feature value in CSV.");
    }
    var trainEnd = rows.Length * 60 / 100;
    var validationEnd = rows.Length * 80 / 100;
    var train = rows[..trainEnd];
    var validation = rows[trainEnd..validationEnd];
    var test = rows[validationEnd..];
    foreach (var split in new[] { train, validation, test })
        if (!split.Any(x => x.IsLate) || !split.Any(x => !x.IsLate))
            throw new InvalidDataException("Each chronological split must contain both classes.");
    var trainingData = ml.Data.LoadFromEnumerable(train);
    var model = DelayModel.Train(ml, trainingData);
    using var engine = ml.Model.CreatePredictionEngine<DeliveryFeatures, DelayOutput>(model);
    var validationScores = validation.Select(x => (Label: x.IsLate, Probability: engine.Predict(x).Probability)).ToArray();
    // Maximize recall subject to validation precision >= 0.6; ties prefer F1 then higher threshold.
    var candidates = Enumerable.Range(1, 19).Select(i => i / 20f)
        .Select(t => (Threshold: t, Metrics: Measure(validationScores, t))).ToArray();
    var feasible = candidates.Where(x => x.Metrics.Precision >= 0.6).ToArray();
    var chosen = feasible.Length > 0
        ? feasible.OrderByDescending(x => x.Metrics.Recall).ThenByDescending(x => x.Metrics.F1).ThenByDescending(x => x.Threshold).First()
        : candidates.OrderByDescending(x => x.Metrics.F1).ThenByDescending(x => x.Threshold).First();
    var scores = test.Select(x => (Label: x.IsLate, Probability: engine.Predict(x).Probability)).ToArray();
    var metrics = Measure(scores, chosen.Threshold);
    var baseline = Measure(test.Select(x => (Label: x.IsLate, Probability: 0f)).ToArray(), chosen.Threshold);
    var brier = scores.Average(x => Math.Pow(x.Probability - (x.Label ? 1 : 0), 2));
    var calibrationBins = Enumerable.Range(0, 5).Select(i =>
    {
        var bin = scores.Where(x => Math.Min(4, (int)(x.Probability * 5)) == i).ToArray();
        return new { Lower = i / 5.0, Upper = (i + 1) / 5.0, Count = bin.Length,
            MeanProbability = bin.Length == 0 ? (double?)null : bin.Average(x => (double)x.Probability),
            ObservedLateRate = bin.Length == 0 ? (double?)null : bin.Average(x => x.Label ? 1.0 : 0.0) };
    });
    var path = Path.GetFullPath(args[1]);
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    ml.Model.Save(model, trainingData.Schema, path);
    var metadata = new ModelMetadata(args[2], chosen.Threshold, DelayModel.Hash(path));
    File.WriteAllText(path + ".json", JsonSerializer.Serialize(metadata, DelayModel.JsonOptions));
    var report = new { metadata.ModelVersion, metadata.DecisionThreshold, Split = new { Train = train.Length, Validation = validation.Length, Test = test.Length },
        ThresholdPolicy = feasible.Length > 0 ? "maximize recall with precision >= 0.6" : "fallback: maximize F1; precision constraint unmet",
        Validation = chosen.Metrics, Test = metrics, AlwaysOnTimeBaseline = baseline, BrierScore = brier, CalibrationBins = calibrationBins,
        Features = new[] { "DistanceKm", "TotalWeightKg", "ServiceType", "OrderHour", "DayOfWeek", "ActiveOrdersAtCreation" },
        Warning = "Sample data is synthetic. Probability estimates require calibration review on representative real data." };
    var json = JsonSerializer.Serialize(report, DelayModel.JsonOptions);
    File.WriteAllText(path + ".metrics.json", json);
    Console.WriteLine(json);
    return 0;
}
catch (Exception ex) when (ex is IOException or InvalidDataException or ArgumentException or InvalidOperationException or FormatException)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

static Metrics Measure((bool Label, float Probability)[] rows, float threshold)
{
    var tp = rows.Count(x => x.Label && x.Probability >= threshold);
    var fp = rows.Count(x => !x.Label && x.Probability >= threshold);
    var fn = rows.Count(x => x.Label && x.Probability < threshold);
    var tn = rows.Length - tp - fp - fn;
    double precision = tp + fp == 0 ? 0 : (double)tp / (tp + fp);
    double recall = tp + fn == 0 ? 0 : (double)tp / (tp + fn);
    return new(tp, fp, fn, tn, precision, recall, precision + recall == 0 ? 0 : 2 * precision * recall / (precision + recall), (double)(tp + tn) / rows.Length);
}

sealed record Metrics(int TruePositive, int FalsePositive, int FalseNegative, int TrueNegative, double Precision, double Recall, double F1, double Accuracy);
