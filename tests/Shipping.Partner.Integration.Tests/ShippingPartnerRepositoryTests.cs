using Xunit;
using Shipping.Partner.Integration.Application;
using Shipping.Partner.Integration.Infrastructure;

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

        store.Append(new ShipmentEventRequest(partnerId, "TRACK123", "In Transit", "Dallas", DateTimeOffset.UtcNow));
        store.Append(new ShipmentEventRequest(Guid.NewGuid(), "TRACK999", "Delivered", null, DateTimeOffset.UtcNow));

        Assert.Single(store.GetByPartnerId(partnerId));
        Assert.Equal(2, store.GetAll().Count);
    }

    [Fact]
    public void ShippingOrderRepository_ShouldCreateAndFilterOrders()
    {
        var repository = new InMemoryShippingOrderRepository();
        var partnerId = Guid.NewGuid();

        var created = repository.Create(new CreateShippingOrderRequest(
            partnerId,
            "SO-1001",
            "Acme Warehouse",
            "123 Main St, Dallas, TX",
            "Ground",
            12.5m));

        Assert.Equal(partnerId, created.PartnerId);
        Assert.Single(repository.GetByPartnerId(partnerId));
        Assert.Single(repository.GetAll());
    }
}
