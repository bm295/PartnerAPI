using Xunit;
using Shipping.Partner.Integration.Application.Requests;
using Shipping.Partner.Integration.Domain.Entities;
using Shipping.Partner.Integration.Domain.Enums;
using Shipping.Partner.Integration.Infrastructure.Repositories;
using Shipping.Partner.Integration.Infrastructure.Stores;

namespace Shipping.Partner.Integration.Tests;

public class ShippingPartnerRepositoryTests
{
    [Fact]
    public void Connect_ShouldCreatePartnerConnection()
    {
        var repository = new InMemoryShippingPartnerRepository();

        var connection = repository.Connect(new ConnectShippingPartnerRequest("FastShip", "fs-001"));

        Assert.Equal("FastShip", connection.Name);
        Assert.Equal("fs-001", connection.ExternalReference);
        Assert.True(repository.Exists(connection.Id));
        Assert.Single(repository.GetAll());
    }

    [Fact]
    public void ShipmentStore_ShouldTrackEventsPerPartner()
    {
        var store = new InMemoryShipmentEventStore();
        var partnerId = Guid.NewGuid();

        store.Append(new ShipmentEventRequest(partnerId, "TRACK123", ShipmentStatus.InTransit, "Dallas", DateTimeOffset.UtcNow));
        store.Append(new ShipmentEventRequest(Guid.NewGuid(), "TRACK999", ShipmentStatus.Delivered, null, DateTimeOffset.UtcNow));

        Assert.Single(store.GetByPartnerId(partnerId));
        Assert.Equal(2, store.GetAll().Count);
    }

    [Fact]
    public void ShipmentStore_ShouldRejectInvalidLifecycleTransitions()
    {
        var store = new InMemoryShipmentEventStore();
        var partnerId = Guid.NewGuid();

        store.Append(new ShipmentEventRequest(partnerId, "TRACK123", ShipmentStatus.LabelCreated, null, DateTimeOffset.UtcNow));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            store.Append(new ShipmentEventRequest(partnerId, "TRACK123", ShipmentStatus.Delivered, null, DateTimeOffset.UtcNow)));

        Assert.Contains("Cannot transition shipment", exception.Message);
    }
}
