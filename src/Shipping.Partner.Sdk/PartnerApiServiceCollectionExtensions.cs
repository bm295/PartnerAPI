using Microsoft.Extensions.DependencyInjection;

namespace Shipping.Partner.Sdk;

public static class PartnerApiServiceCollectionExtensions
{
    public static IServiceCollection AddPartnerApi(this IServiceCollection services, Action<PartnerApiOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new PartnerApiOptions();
        configure(options);
        if (options.BaseUrl is null || !options.BaseUrl.IsAbsoluteUri || options.BaseUrl.Scheme is not ("http" or "https"))
            throw new ArgumentException("Partner API BaseUrl must be an absolute HTTP(S) URL.", nameof(configure));
        if (string.IsNullOrWhiteSpace(options.ApiKey))
            throw new ArgumentException("Partner API ApiKey is required.", nameof(configure));
        if (options.Timeout <= TimeSpan.Zero)
            throw new ArgumentException("Partner API Timeout must be positive.", nameof(configure));

        services.AddHttpClient<IPartnerApiClient, PartnerApiClient>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl.AbsoluteUri.TrimEnd('/') + "/");
            client.Timeout = options.Timeout;
            client.DefaultRequestHeaders.Add("X-Shipping-Partner-Key", options.ApiKey);
        });
        return services;
    }
}
