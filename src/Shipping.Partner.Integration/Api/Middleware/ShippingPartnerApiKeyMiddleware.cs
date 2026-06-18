using Microsoft.Extensions.Options;
using Shipping.Partner.Integration.Application.Abstractions;
using Shipping.Partner.Integration.Application.Configuration;

namespace Shipping.Partner.Integration.Api.Middleware;

public sealed class ShippingPartnerApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOptions<ShippingPartnerIntegrationOptions> _options;
    private readonly IApiKeyValidator _validator;

    public ShippingPartnerApiKeyMiddleware(
        RequestDelegate next,
        IOptions<ShippingPartnerIntegrationOptions> options,
        IApiKeyValidator validator)
    {
        _next = next;
        _options = options;
        _validator = validator;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase) ||
            context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        var headerName = _options.Value.ApiKeyHeaderName;
        if (!context.Request.Headers.TryGetValue(headerName, out var apiKey) || string.IsNullOrWhiteSpace(apiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = $"Missing {headerName} header." }).ConfigureAwait(false);
            return;
        }

        if (!_validator.IsValid(apiKey!))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid shipping partner API key." }).ConfigureAwait(false);
            return;
        }

        await _next(context).ConfigureAwait(false);
    }
}
