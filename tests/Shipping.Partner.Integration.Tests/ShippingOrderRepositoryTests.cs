using Xunit;
using Shipping.Partner.Integration.Application.Requests;
using Shipping.Partner.Integration.Infrastructure;

namespace Shipping.Partner.Integration.Tests;

public class ShippingOrderRepositoryTests
{
    [Fact]
    public void Create_ShouldCreateAndFilterOrders()
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

        Assert.True(created.Created);
        Assert.Equal(partnerId, created.Order.PartnerId);
        Assert.Single(repository.GetByPartnerId(partnerId));
        Assert.Single(repository.GetAll());
    }

    [Fact]
    public void Create_ShouldReturnExistingOrder_ForSamePartnerAndOrderNumber()
    {
        var repository = new InMemoryShippingOrderRepository();
        var partnerId = Guid.NewGuid();

        var first = repository.Create(new CreateShippingOrderRequest(
            partnerId,
            "SO-1001",
            "Acme Warehouse",
            "123 Main St, Dallas, TX",
            "Ground",
            12.5m));
        var second = repository.Create(new CreateShippingOrderRequest(
            partnerId,
            " so-1001 ",
            "Changed Name",
            "999 Changed St, Dallas, TX",
            "Express",
            20m));

        Assert.True(first.Created);
        Assert.False(second.Created);
        Assert.Equal(first.Order, second.Order);
        Assert.Single(repository.GetByPartnerId(partnerId));
        Assert.Single(repository.GetAll());
    }

    [Fact]
    public void Create_ShouldCreateSeparateOrders_ForDifferentPartnersWithSameOrderNumber()
    {
        var repository = new InMemoryShippingOrderRepository();

        var first = repository.Create(new CreateShippingOrderRequest(
            Guid.NewGuid(),
            "SO-1001",
            "Acme Warehouse",
            "123 Main St, Dallas, TX",
            "Ground",
            12.5m));
        var second = repository.Create(new CreateShippingOrderRequest(
            Guid.NewGuid(),
            "SO-1001",
            "Acme Warehouse",
            "123 Main St, Dallas, TX",
            "Ground",
            12.5m));

        Assert.True(first.Created);
        Assert.True(second.Created);
        Assert.NotEqual(first.Order.Id, second.Order.Id);
        Assert.Equal(2, repository.GetAll().Count);
    }
}
