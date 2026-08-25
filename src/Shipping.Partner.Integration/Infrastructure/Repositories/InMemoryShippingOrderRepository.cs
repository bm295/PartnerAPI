using System.Collections.Concurrent;
using Shipping.Partner.Integration.Application.Abstractions;
using Shipping.Partner.Integration.Application.Models;
using Shipping.Partner.Integration.Application.Results;
using Shipping.Partner.Integration.Domain.Entities;

namespace Shipping.Partner.Integration.Infrastructure.Repositories;

public sealed class InMemoryShippingOrderRepository : IShippingOrderRepository
{
    private readonly ConcurrentDictionary<Guid, ShippingOrder> _orders = new();
    private readonly ConcurrentDictionary<string, Guid> _orderIdsByIdempotencyKey = new();
    private readonly object _createLock = new();

    public IReadOnlyCollection<ShippingOrder> GetAll() =>
        _orders.Values.OrderBy(order => order.CreatedAtUtc).ToArray();

    public IReadOnlyCollection<ShippingOrder> GetByPartnerId(Guid partnerId) =>
        _orders.Values.Where(order => order.PartnerId == partnerId).OrderBy(order => order.CreatedAtUtc).ToArray();

    public ShippingOrderCreationResult Create(NewShippingOrder order)
    {
        var orderNumber = order.OrderNumber.Trim();
        var key = CreateIdempotencyKey(order.PartnerId, orderNumber);

        lock (_createLock)
        {
            if (_orderIdsByIdempotencyKey.TryGetValue(key, out var existingOrderId) &&
                _orders.TryGetValue(existingOrderId, out var existingOrder))
            {
                return new ShippingOrderCreationResult(existingOrder, false);
            }

            var storedOrder = new ShippingOrder(
                Guid.NewGuid(),
                order.PartnerId,
                orderNumber,
                order.DestinationName.Trim(),
                order.DestinationAddress.Trim(),
                order.ServiceLevel.Trim(),
                order.TotalWeightKg,
                DateTimeOffset.UtcNow);

            _orders[storedOrder.Id] = storedOrder;
            _orderIdsByIdempotencyKey[key] = storedOrder.Id;
            return new ShippingOrderCreationResult(storedOrder, true);
        }
    }

    private static string CreateIdempotencyKey(Guid partnerId, string orderNumber) =>
        $"{partnerId:N}:{orderNumber.ToUpperInvariant()}";
}
