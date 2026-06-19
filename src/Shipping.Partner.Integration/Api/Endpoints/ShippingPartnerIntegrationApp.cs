using Shipping.Partner.Integration.Api.Middleware;
using Shipping.Partner.Integration.Application.Commands;
using Shipping.Partner.Integration.Application.Cqrs;
using Shipping.Partner.Integration.Application.Queries;
using Shipping.Partner.Integration.Application.Requests;
using Shipping.Partner.Integration.Application.Results;
using Shipping.Partner.Integration.Domain;

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
            ICommandHandler<ConnectShippingPartnerCommand, ShippingPartnerConnection> handler) =>
        {
            var partner = handler.Handle(new ConnectShippingPartnerCommand(request.Name, request.ExternalReference));
            return Results.Created($"/shipping-partners/{partner.Id}", partner);
        });

        app.MapGet("/shipping-partners", (
            IQueryHandler<GetShippingPartnersQuery, IReadOnlyCollection<ShippingPartnerConnection>> handler) =>
            Results.Ok(handler.Handle(new GetShippingPartnersQuery())));

        app.MapGet("/shipping-partners/{id:guid}", (
            Guid id,
            IQueryHandler<GetShippingPartnerByIdQuery, ShippingPartnerConnection?> handler) =>
        {
            var partner = handler.Handle(new GetShippingPartnerByIdQuery(id));
            return partner is null ? Results.NotFound() : Results.Ok(partner);
        });

        app.MapPost("/shipments/events", (
            ShipmentEventRequest request,
            ICommandHandler<RecordShipmentEventCommand, CommandResult<ShipmentEventRecord>> handler) =>
        {
            var result = handler.Handle(new RecordShipmentEventCommand(
                request.PartnerId,
                request.TrackingNumber,
                request.Status,
                request.Location,
                request.OccurredAtUtc));

            return result.Succeeded
                ? Results.Accepted($"/shipments/events/{result.Value!.Id}", result.Value)
                : Results.BadRequest(new { error = result.Error });
        });

        app.MapGet("/shipments/events", (
            IQueryHandler<GetShipmentEventsQuery, IReadOnlyCollection<ShipmentEventRecord>> handler,
            Guid? partnerId) => Results.Ok(handler.Handle(new GetShipmentEventsQuery(partnerId))));

        app.MapPost("/shipping-orders", (
            CreateShippingOrderRequest request,
            ICommandHandler<CreateShippingOrderCommand, CommandResult<ShippingOrderCreationResult>> handler) =>
        {
            var result = handler.Handle(new CreateShippingOrderCommand(
                request.PartnerId,
                request.OrderNumber,
                request.DestinationName,
                request.DestinationAddress,
                request.ServiceLevel,
                request.TotalWeightKg));

            if (!result.Succeeded)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            var orderResult = result.Value!;
            return orderResult.Created
                ? Results.Created($"/shipping-orders/{orderResult.Order.Id}", orderResult.Order)
                : Results.Ok(orderResult.Order);
        });

        app.MapGet("/shipping-orders", (
            IQueryHandler<GetShippingOrdersQuery, IReadOnlyCollection<ShippingOrder>> handler,
            Guid? partnerId) => Results.Ok(handler.Handle(new GetShippingOrdersQuery(partnerId))));

        return app;
    }
}
