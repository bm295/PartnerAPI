using Shipping.Partner.Integration.Application.Abstractions;
using Shipping.Partner.Integration.Application.Commands;
using Shipping.Partner.Integration.Application.Handlers;
using Shipping.Partner.Integration.Application.Models;
using Shipping.Partner.Integration.Application.Results;
using Shipping.Partner.Integration.Domain.Entities;
using Shipping.Partner.Integration.Domain.Enums;
using Xunit;

namespace Shipping.Partner.Integration.Tests;

public class CommandHandlerCharacterizationTests
{
    [Fact]
    public void CreateOrder_ShouldRejectUnknownPartnerWithoutWritingOrder()
    {
        var orders = new RecordingOrderRepository();
        var handler = new CreateShippingOrderCommandHandler(new MissingPartnerRepository(), orders);

        var result = handler.Handle(new CreateShippingOrderCommand(
            Guid.NewGuid(), "SO-1", "Warehouse", "1 Main St", "Ground", 2.5m));

        Assert.False(result.Succeeded);
        Assert.Equal("Unknown partner.", result.Error);
        Assert.Null(orders.ReceivedOrder);
    }

    [Fact]
    public void CreateOrder_ShouldMapCommandToPersistenceModel()
    {
        var partnerId = Guid.NewGuid();
        var orders = new RecordingOrderRepository();
        var handler = new CreateShippingOrderCommandHandler(new ExistingPartnerRepository(), orders);

        var result = handler.Handle(new CreateShippingOrderCommand(
            partnerId, " SO-1 ", " Warehouse ", " 1 Main St ", " Ground ", 2.5m));

        Assert.True(result.Succeeded);
        Assert.NotNull(orders.ReceivedOrder);
        Assert.Equal(partnerId, orders.ReceivedOrder.PartnerId);
        Assert.Equal(" SO-1 ", orders.ReceivedOrder.OrderNumber);
        Assert.Equal(" Warehouse ", orders.ReceivedOrder.DestinationName);
        Assert.Equal(" 1 Main St ", orders.ReceivedOrder.DestinationAddress);
        Assert.Equal(" Ground ", orders.ReceivedOrder.ServiceLevel);
        Assert.Equal(2.5m, orders.ReceivedOrder.TotalWeightKg);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void CreateOrder_ShouldRejectNonPositiveWeightWithoutWritingOrder(double weight)
    {
        var orders = new RecordingOrderRepository();
        var handler = new CreateShippingOrderCommandHandler(new ExistingPartnerRepository(), orders);

        var result = handler.Handle(new CreateShippingOrderCommand(
            Guid.NewGuid(), "SO-1", "Warehouse", "1 Main St", "Ground", (decimal)weight));

        Assert.False(result.Succeeded);
        Assert.Equal("TotalWeightKg must be greater than zero.", result.Error);
        Assert.Null(orders.ReceivedOrder);
    }

    [Fact]
    public void RecordShipmentEvent_ShouldRejectInvalidStatusWithoutAppending()
    {
        var events = new RecordingShipmentEventStore();
        var handler = new RecordShipmentEventCommandHandler(events, new ExistingPartnerRepository());

        var result = handler.Handle(new RecordShipmentEventCommand(
            Guid.NewGuid(), "TRACK-1", "not-a-status", "Dallas", DateTimeOffset.UtcNow));

        Assert.False(result.Succeeded);
        Assert.Equal("Invalid shipment status.", result.Error);
        Assert.Null(events.ReceivedEvent);
    }

    [Fact]
    public void RecordShipmentEvent_ShouldParseStatusAndMapPersistenceModel()
    {
        var partnerId = Guid.NewGuid();
        var occurredAtUtc = DateTimeOffset.UtcNow.AddMinutes(-5);
        var events = new RecordingShipmentEventStore();
        var handler = new RecordShipmentEventCommandHandler(events, new ExistingPartnerRepository());

        var result = handler.Handle(new RecordShipmentEventCommand(
            partnerId, " TRACK-1 ", "intransit", " Dallas ", occurredAtUtc));

        Assert.True(result.Succeeded);
        Assert.NotNull(events.ReceivedEvent);
        Assert.Equal(partnerId, events.ReceivedEvent.PartnerId);
        Assert.Equal(" TRACK-1 ", events.ReceivedEvent.TrackingNumber);
        Assert.Equal(ShipmentStatus.InTransit, events.ReceivedEvent.Status);
        Assert.Equal(" Dallas ", events.ReceivedEvent.Location);
        Assert.Equal(occurredAtUtc, events.ReceivedEvent.OccurredAtUtc);
    }

    [Fact]
    public void ConnectPartner_ShouldMapCommandToPersistenceModel()
    {
        var partners = new RecordingPartnerRepository();
        var handler = new ConnectShippingPartnerCommandHandler(partners);

        var result = handler.Handle(new ConnectShippingPartnerCommand(" FastShip ", " fs-1 "));

        Assert.NotNull(partners.ReceivedPartner);
        Assert.Equal(" FastShip ", partners.ReceivedPartner.Name);
        Assert.Equal(" fs-1 ", partners.ReceivedPartner.ExternalReference);
        Assert.Equal(partners.CreatedPartner, result);
    }

    private sealed class RecordingOrderRepository : IShippingOrderRepository
    {
        public NewShippingOrder? ReceivedOrder { get; private set; }

        public IReadOnlyCollection<ShippingOrder> GetAll() => [];

        public IReadOnlyCollection<ShippingOrder> GetByPartnerId(Guid partnerId) => [];

        public ShippingOrderCreationResult Create(NewShippingOrder order)
        {
            ReceivedOrder = order;
            var storedOrder = new ShippingOrder(
                Guid.NewGuid(), order.PartnerId, order.OrderNumber, order.DestinationName,
                order.DestinationAddress, order.ServiceLevel, order.TotalWeightKg, DateTimeOffset.UtcNow);
            return new ShippingOrderCreationResult(storedOrder, true);
        }
    }

    private sealed class RecordingShipmentEventStore : IShipmentEventStore
    {
        public NewShipmentEvent? ReceivedEvent { get; private set; }

        public IReadOnlyCollection<ShipmentEventRecord> GetAll() => [];

        public IReadOnlyCollection<ShipmentEventRecord> GetByPartnerId(Guid partnerId) => [];

        public ShipmentEventRecord Append(NewShipmentEvent shipmentEvent)
        {
            ReceivedEvent = shipmentEvent;
            return new ShipmentEventRecord(
                Guid.NewGuid(), shipmentEvent.PartnerId, shipmentEvent.TrackingNumber, shipmentEvent.Status,
                shipmentEvent.Location, shipmentEvent.OccurredAtUtc, DateTimeOffset.UtcNow);
        }
    }

    private sealed class RecordingPartnerRepository : IShippingPartnerRepository
    {
        public NewShippingPartner? ReceivedPartner { get; private set; }

        public ShippingPartnerConnection CreatedPartner { get; } =
            new(Guid.NewGuid(), "FastShip", "fs-1", "key", DateTimeOffset.UtcNow);

        public IReadOnlyCollection<ShippingPartnerConnection> GetAll() => [];

        public ShippingPartnerConnection? GetById(Guid id) => null;

        public ShippingPartnerConnection Connect(NewShippingPartner partner)
        {
            ReceivedPartner = partner;
            return CreatedPartner;
        }

        public bool Exists(Guid id) => false;
    }

    private sealed class MissingPartnerRepository : PartnerRepositoryStub
    {
        public override bool Exists(Guid id) => false;
    }

    private sealed class ExistingPartnerRepository : PartnerRepositoryStub
    {
        public override bool Exists(Guid id) => true;
    }

    private abstract class PartnerRepositoryStub : IShippingPartnerRepository
    {
        public IReadOnlyCollection<ShippingPartnerConnection> GetAll() => [];
        public ShippingPartnerConnection? GetById(Guid id) => null;
        public ShippingPartnerConnection Connect(NewShippingPartner partner) => throw new NotSupportedException();
        public abstract bool Exists(Guid id);
    }
}
