using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shipping.Partner.Integration.Api.Middleware;
using Shipping.Partner.Integration.Infrastructure;

namespace Shipping.Partner.Integration.Application;

public static class ShippingPartnerIntegrationServiceCollectionExtensions
{
    public static IServiceCollection AddShippingPartnerIntegration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.Configure<ShippingPartnerIntegrationOptions>(
            configuration.GetSection(ShippingPartnerIntegrationOptions.SectionName));
        services.AddSingleton<IShippingPartnerRepository, InMemoryShippingPartnerRepository>();
        services.AddSingleton<IShipmentEventStore, InMemoryShipmentEventStore>();
        services.AddSingleton<IShippingOrderRepository, InMemoryShippingOrderRepository>();
        services.AddSingleton<IApiKeyValidator, ConfigurationApiKeyValidator>();
        services.AddSingleton<ShippingPartnerApiKeyMiddleware>();
        services.AddAuthorization();
        return services;
    }
}
