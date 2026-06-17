using Shipping.Partner.Integration.Api.Middleware;
using Shipping.Partner.Integration.Application;

namespace Shipping.Partner.Integration.Api.Endpoints;

public static class ShippingPartnerIntegrationApp
{
    public static WebApplication UseShippingPartnerIntegration(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseMiddleware<ShippingPartnerApiKeyMiddleware>();
        app.UseAuthorization();

        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

        app.MapPost("/shipping-partners/connect", (
            ConnectShippingPartnerRequest request,
            IShippingPartnerRepository repository) =>
        {
            var partner = repository.Connect(request);
            return Results.Created($"/shipping-partners/{partner.Id}", partner);
        });

        app.MapGet("/shipping-partners", (IShippingPartnerRepository repository) =>
            Results.Ok(repository.GetAll()));

        app.MapGet("/shipping-partners/{id:guid}", (Guid id, IShippingPartnerRepository repository) =>
        {
            var partner = repository.GetById(id);
            return partner is null ? Results.NotFound() : Results.Ok(partner);
        });

        app.MapPost("/shipments/events", (
            ShipmentEventRequest request,
            IShipmentEventStore store,
            IShippingPartnerRepository repository) =>
        {
            if (!repository.Exists(request.PartnerId))
            {
                return Results.BadRequest(new { error = "Unknown partner." });
            }

            var eventRecord = store.Append(request);
            return Results.Accepted($"/shipments/events/{eventRecord.Id}", eventRecord);
        });

        app.MapGet("/shipments/events", (IShipmentEventStore store, Guid? partnerId) =>
        {
            var events = partnerId is null ? store.GetAll() : store.GetByPartnerId(partnerId.Value);
            return Results.Ok(events);
        });

        app.MapPost("/shipping-orders", (
            CreateShippingOrderRequest request,
            IShippingPartnerRepository partnerRepository,
            IShippingOrderRepository orderRepository) =>
        {
            if (!partnerRepository.Exists(request.PartnerId))
            {
                return Results.BadRequest(new { error = "Unknown partner." });
            }

            if (request.TotalWeightKg <= 0)
            {
                return Results.BadRequest(new { error = "TotalWeightKg must be greater than zero." });
            }

            var order = orderRepository.Create(request);
            return Results.Created($"/shipping-orders/{order.Id}", order);
        });

        app.MapGet("/shipping-orders", (IShippingOrderRepository orderRepository, Guid? partnerId) =>
        {
            var orders = partnerId is null ? orderRepository.GetAll() : orderRepository.GetByPartnerId(partnerId.Value);
            return Results.Ok(orders);
        });

        return app;
    }
}
