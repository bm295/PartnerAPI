using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.ML;
using Shipping.DelayPrediction;
using Shipping.Partner.Integration.Api.DependencyInjection;
using Shipping.Partner.Integration.Api.Endpoints;
using Shipping.Partner.Integration.Application.Abstractions;
using Shipping.Partner.Integration.Application.Handlers;
using Shipping.Partner.Integration.Application.Queries;
using Shipping.Partner.Integration.Application.Results;
using Shipping.Partner.Integration.Infrastructure.MachineLearning;
using Xunit;

namespace Shipping.Partner.Integration.Tests;

public sealed class ModelFixture : IDisposable
{
    public string DirectoryPath { get; } = Path.Combine(Path.GetTempPath(), "delay-tests-" + Guid.NewGuid());
    public string ModelPath => Path.Combine(DirectoryPath, "model.zip");
    public ModelFixture()
    {
        Directory.CreateDirectory(DirectoryPath);
        var ml = new MLContext(seed: 42);
        var rows = Enumerable.Range(0, 200).Select(i => new DeliveryFeatures
        {
            DistanceKm = i % 2 == 0 ? 2 : 25, TotalWeightKg = 1,
            ServiceType = i % 3 == 0 ? "express" : "standard", OrderHour = i % 24,
            DayOfWeek = i % 7, ActiveOrdersAtCreation = i % 2 == 0 ? 1 : 70, IsLate = i % 2 != 0
        });
        var data = ml.Data.LoadFromEnumerable(rows);
        ml.Model.Save(DelayModel.Train(ml, data), data.Schema, ModelPath);
        File.WriteAllText(ModelPath + ".json", JsonSerializer.Serialize(new ModelMetadata("test-v1", .6f, DelayModel.Hash(ModelPath))));
    }
    public void Dispose() => Directory.Delete(DirectoryPath, recursive: true);
}

public sealed class DeliveryDelayPredictionTests(ModelFixture fixture) : IClassFixture<ModelFixture>
{
    private static PredictDeliveryDelayQuery Valid => new(8.5f, 1.2f, "standard", 18, 5, 24);

    [Theory]
    [InlineData(0, 1, "standard", 1, 1, 0)]
    [InlineData(1, -1, "standard", 1, 1, 0)]
    [InlineData(1, 1, null, 1, 1, 0)]
    [InlineData(1, 1, "unknown", 1, 1, 0)]
    [InlineData(1, 1, "standard", 24, 1, 0)]
    [InlineData(1, 1, "standard", 1, 7, 0)]
    [InlineData(1, 1, "standard", 1, 1, -1)]
    [InlineData(float.NaN, 1, "standard", 1, 1, 0)]
    [InlineData(float.PositiveInfinity, 1, "standard", 1, 1, 0)]
    public void RejectsInvalidFeaturesBeforeCallingModel(float distance, float weight, string? service, int hour, int day, int active)
    {
        var handler = new PredictDeliveryDelayQueryHandler(new NeverCalledPredictor());
        Assert.Throws<ArgumentException>(() => handler.Handle(new(distance, weight, service, hour, day, active)));
    }

    [Fact]
    public async Task SavedModelSupportsConcurrentRequestsAndThresholdMetadata()
    {
        var predictor = new MlNetDeliveryDelayPredictor(fixture.ModelPath, NullLogger<MlNetDeliveryDelayPredictor>.Instance);
        var outputs = await Task.WhenAll(Enumerable.Range(0, 16).Select(_ => Task.Run(() => predictor.Predict(Valid))));
        foreach (var output in outputs)
        {
            Assert.InRange(output.DelayProbability, 0, 1);
            Assert.Equal("test-v1", output.ModelVersion);
            Assert.Equal(.6f, output.DecisionThreshold);
            Assert.Equal(output.DelayProbability >= .6f, output.PredictedLate);
            Assert.Equal(outputs[0], output);
        }
    }

    [Fact]
    public void LearnedModelDistinguishesLowAndHighRiskFeatures()
    {
        var predictor = new MlNetDeliveryDelayPredictor(fixture.ModelPath, NullLogger<MlNetDeliveryDelayPredictor>.Instance);
        var low = predictor.Predict(Valid with { DistanceKm = 2, ActiveOrdersAtCreation = 1 });
        var high = predictor.Predict(Valid with { DistanceKm = 25, ActiveOrdersAtCreation = 70 });
        Assert.True(high.DelayProbability > low.DelayProbability);
    }

    [Fact]
    public void ChecksumMismatchIsRejected()
    {
        var path = Path.Combine(fixture.DirectoryPath, "corrupt.zip");
        File.Copy(fixture.ModelPath, path);
        File.Copy(fixture.ModelPath + ".json", path + ".json");
        using (var stream = File.OpenWrite(path)) { stream.WriteByte(0); }
        var predictor = new MlNetDeliveryDelayPredictor(path, NullLogger<MlNetDeliveryDelayPredictor>.Instance);
        Assert.Throws<PredictionUnavailableException>(() => predictor.Predict(Valid));
    }

    [Fact]
    public async Task HttpEndpointUsesRealModelAndRequiresApiKey()
    {
        var app = await Start(fixture.ModelPath);
        await using var appLifetime = app.ConfigureAwait(true);
        using var client = app.GetTestClient();
        var unauthorized = await client.PostAsJsonAsync("/shipping-orders/predict-delay", Valid);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);
        client.DefaultRequestHeaders.Add("X-Shipping-Partner-Key", "bad");
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/shipping-orders/predict-delay", Valid)).StatusCode);
        client.DefaultRequestHeaders.Remove("X-Shipping-Partner-Key");
        client.DefaultRequestHeaders.Add("X-Shipping-Partner-Key", "test-key");
        var response = await client.PostAsJsonAsync("/shipping-orders/predict-delay", Valid);
        response.EnsureSuccessStatusCode();
        var prediction = await response.Content.ReadFromJsonAsync<DeliveryDelayPrediction>();
        Assert.Equal("test-v1", prediction!.ModelVersion);
        Assert.InRange(prediction.DelayProbability, 0, 1);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/shipping-orders/predict-delay", Valid with { OrderHour = 24 })).StatusCode);
        using var missing = new StringContent("{\"distanceKm\":8.5}", Encoding.UTF8, "application/json");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/shipping-orders/predict-delay", missing)).StatusCode);
    }

    [Fact]
    public async Task MissingModelReturns503WithoutBreakingHealth()
    {
        var app = await Start(Path.Combine(fixture.DirectoryPath, "missing.zip"));
        await using var appLifetime = app.ConfigureAwait(true);
        using var client = app.GetTestClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);
        client.DefaultRequestHeaders.Add("X-Shipping-Partner-Key", "test-key");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await client.PostAsJsonAsync("/shipping-orders/predict-delay", Valid)).StatusCode);
    }

    private static async Task<WebApplication> Start(string path)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["DeliveryDelayPrediction:ModelPath"] = path });
        builder.Services.AddShippingPartnerIntegration(builder.Configuration);
        builder.Services.AddSingleton<IApiKeyValidator, TestValidator>();
        var app = builder.Build();
        app.UseShippingPartnerIntegration();
        await app.StartAsync().ConfigureAwait(true);
        return app;
    }
    private sealed class TestValidator : IApiKeyValidator { public bool IsValid(string apiKey) => apiKey == "test-key"; }
    private sealed class NeverCalledPredictor : IDeliveryDelayPredictor
    {
        public DeliveryDelayPrediction Predict(PredictDeliveryDelayQuery input) => throw new InvalidOperationException("Validation must precede inference.");
    }
}
