using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Shipping.Partner.Sdk;
using Shipping.Partner.Sdk.Models;
using Xunit;

namespace Shipping.Partner.Integration.Tests;

public sealed class PartnerApiClientTests
{
    [Fact]
    public async Task CreateOrderSendsApiKeyAndReturnsOrder()
    {
        var expected = new ShippingOrder(Guid.NewGuid(), Guid.NewGuid(), "A-1", "Ada", "Main St", "standard", 1.2m, DateTimeOffset.UtcNow);
        var handler = new StubHandler(async request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/shipping-orders", request.RequestUri!.AbsolutePath);
            Assert.Equal("secret", request.Headers.GetValues("X-Shipping-Partner-Key").Single());
            var payload = await request.Content!.ReadFromJsonAsync<CreateShippingOrderRequest>().ConfigureAwait(false);
            Assert.Equal(expected.OrderNumber, payload!.OrderNumber);
            return new HttpResponseMessage(HttpStatusCode.Created) { Content = JsonContent.Create(expected) };
        });
        var client = CreateClient(handler);

        var actual = await client.CreateOrderAsync(new CreateShippingOrderRequest(expected.PartnerId, expected.OrderNumber, expected.DestinationName, expected.DestinationAddress, expected.ServiceLevel, expected.TotalWeightKg));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetOrdersFiltersByPartnerAndReturnsOrders()
    {
        var partnerId = Guid.NewGuid();
        var handler = new StubHandler(request =>
        {
            Assert.Equal($"?partnerId={partnerId:D}", request.RequestUri!.Query);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(Array.Empty<ShippingOrder>()) });
        });

        Assert.Empty(await CreateClient(handler).GetOrdersAsync(partnerId));
    }

    [Fact]
    public async Task ApiErrorIncludesStatusAndMessage()
    {
        var handler = new StubHandler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = JsonContent.Create(new { error = "Invalid order" })
        }));

        var error = await Assert.ThrowsAsync<PartnerApiException>(() => CreateClient(handler).GetOrdersAsync(Guid.NewGuid()));
        Assert.Equal(HttpStatusCode.BadRequest, error.StatusCode);
        Assert.Equal("Invalid order", error.Message);
    }

    private static IPartnerApiClient CreateClient(HttpMessageHandler handler)
    {
        var services = new ServiceCollection();
        services.AddPartnerApi(options => { options.BaseUrl = new Uri("https://example.test/"); options.ApiKey = "secret"; });
        services.AddSingleton<IHttpMessageHandlerBuilderFilter>(new HandlerFilter(handler));
        return services.BuildServiceProvider().GetRequiredService<IPartnerApiClient>();
    }

    private sealed class StubHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => respond(request);
    }

    private sealed class HandlerFilter(HttpMessageHandler handler) : IHttpMessageHandlerBuilderFilter
    {
        public Action<Microsoft.Extensions.Http.HttpMessageHandlerBuilder> Configure(Action<Microsoft.Extensions.Http.HttpMessageHandlerBuilder> next) => builder =>
        {
            next(builder);
            builder.PrimaryHandler = handler;
        };
    }
}
