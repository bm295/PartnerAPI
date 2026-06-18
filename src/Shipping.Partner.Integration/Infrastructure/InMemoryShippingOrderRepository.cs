using System.Collections.Concurrent;
using Shipping.Partner.Integration.Application.Abstractions;
using Shipping.Partner.Integration.Application.Requests;
using Shipping.Partner.Integration.Domain;

namespace Shipping.Partner.Integration.Infrastructure;

public sealed class InMemoryShippingOrderRepository : IShippingOrderRepository
{
    private readonly ConcurrentDictionary<Guid, ShippingOrder> _orders = new();

    public IReadOnlyCollection<ShippingOrder> GetAll() =>
        _orders.Values.OrderBy(order => order.CreatedAtUtc).ToArray();

    public IReadOnlyCollection<ShippingOrder> GetByPartnerId(Guid partnerId) =>
        _orders.Values.Where(order => order.PartnerId == partnerId).OrderBy(order => order.CreatedAtUtc).ToArray();

    public ShippingOrder Create(CreateShippingOrderRequest request)
    {
        var order = new ShippingOrder(
            Guid.NewGuid(),
            request.PartnerId,
            request.OrderNumber.Trim(),
            request.DestinationName.Trim(),
            request.DestinationAddress.Trim(),
            request.ServiceLevel.Trim(),
            request.TotalWeightKg,
            DateTimeOffset.UtcNow);

        _orders[order.Id] = order;
        return order;
    }
}
