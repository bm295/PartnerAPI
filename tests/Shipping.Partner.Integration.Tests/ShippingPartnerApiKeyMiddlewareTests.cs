using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Shipping.Partner.Integration.Api.Middleware;
using Shipping.Partner.Integration.Application.Abstractions;
using Shipping.Partner.Integration.Application.Configuration;
using Xunit;

namespace Shipping.Partner.Integration.Tests;

public class ShippingPartnerApiKeyMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldUseCredentialHeaderConfiguration()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            new PartnerCredentialOptions { HeaderName = "X-Custom-Partner-Key" },
            new StubApiKeyValidator(true));
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Custom-Partner-Key"] = "secret";

        await middleware.InvokeAsync(context).ConfigureAwait(false);

        Assert.True(nextCalled);
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/swagger/index.html")]
    public async Task InvokeAsync_ShouldBypassAuthenticationForPublicPaths(string path)
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            new PartnerCredentialOptions(),
            new StubApiKeyValidator(false));
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        await middleware.InvokeAsync(context).ConfigureAwait(false);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ShouldRejectMissingConfiguredHeader()
    {
        var middleware = CreateMiddleware(
            _ => Task.CompletedTask,
            new PartnerCredentialOptions { HeaderName = "X-Custom-Partner-Key" },
            new StubApiKeyValidator(true));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context).ConfigureAwait(false);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        Assert.Contains("Missing X-Custom-Partner-Key header.", await reader.ReadToEndAsync().ConfigureAwait(false));
    }

    [Fact]
    public async Task InvokeAsync_ShouldRejectInvalidKeyWithoutCallingNextMiddleware()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            new PartnerCredentialOptions(),
            new StubApiKeyValidator(false));
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Shipping-Partner-Key"] = "invalid";

        await middleware.InvokeAsync(context).ConfigureAwait(false);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    private static ShippingPartnerApiKeyMiddleware CreateMiddleware(
        RequestDelegate next,
        PartnerCredentialOptions options,
        IApiKeyValidator validator) =>
        new(next, Options.Create(options), validator);

    private sealed class StubApiKeyValidator(bool isValid) : IApiKeyValidator
    {
        public bool IsValid(string apiKey) => isValid;
    }
}
