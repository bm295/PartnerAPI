using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shipping.Partner.Integration.Api.Middleware;
using Shipping.Partner.Integration.Application.Abstractions;
using Shipping.Partner.Integration.Application.Commands;
using Shipping.Partner.Integration.Application.Configuration;
using Shipping.Partner.Integration.Application.Cqrs;
using Shipping.Partner.Integration.Application.Handlers;
using Shipping.Partner.Integration.Application.Queries;
using Shipping.Partner.Integration.Application.Results;
using Shipping.Partner.Integration.Domain;
using Shipping.Partner.Integration.Infrastructure;

namespace Shipping.Partner.Integration.Application.DependencyInjection;

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
        services.AddSingleton<ICommandHandler<ConnectShippingPartnerCommand, ShippingPartnerConnection>, ConnectShippingPartnerCommandHandler>();
        services.AddSingleton<ICommandHandler<RecordShipmentEventCommand, CommandResult<ShipmentEventRecord>>, RecordShipmentEventCommandHandler>();
        services.AddSingleton<ICommandHandler<CreateShippingOrderCommand, CommandResult<ShippingOrderCreationResult>>, CreateShippingOrderCommandHandler>();
        services.AddSingleton<IQueryHandler<GetShippingPartnersQuery, IReadOnlyCollection<ShippingPartnerConnection>>, GetShippingPartnersQueryHandler>();
        services.AddSingleton<IQueryHandler<GetShippingPartnerByIdQuery, ShippingPartnerConnection?>, GetShippingPartnerByIdQueryHandler>();
        services.AddSingleton<IQueryHandler<GetShipmentEventsQuery, IReadOnlyCollection<ShipmentEventRecord>>, GetShipmentEventsQueryHandler>();
        services.AddSingleton<IQueryHandler<GetShippingOrdersQuery, IReadOnlyCollection<ShippingOrder>>, GetShippingOrdersQueryHandler>();
        services.AddSingleton<ShippingPartnerApiKeyMiddleware>();
        services.AddAuthorization();
        return services;
    }
}
