using System.Collections.Concurrent;
using Shipping.Partner.Integration.Application.Abstractions;
using Shipping.Partner.Integration.Application.Requests;
using Shipping.Partner.Integration.Application.Results;
using Shipping.Partner.Integration.Domain;

namespace Shipping.Partner.Integration.Infrastructure;

public sealed class InMemoryShippingOrderRepository : IShippingOrderRepository
{
    private readonly ConcurrentDictionary<Guid, ShippingOrder> _orders = new();
    private readonly ConcurrentDictionary<string, Guid> _orderIdsByIdempotencyKey = new();
    private readonly object _createLock = new();

    public IReadOnlyCollection<ShippingOrder> GetAll() =>
        _orders.Values.OrderBy(order => order.CreatedAtUtc).ToArray();

    public IReadOnlyCollection<ShippingOrder> GetByPartnerId(Guid partnerId) =>
        _orders.Values.Where(order => order.PartnerId == partnerId).OrderBy(order => order.CreatedAtUtc).ToArray();

    public ShippingOrderCreationResult Create(CreateShippingOrderRequest request)
    {
        var orderNumber = request.OrderNumber.Trim();
        var key = CreateIdempotencyKey(request.PartnerId, orderNumber);

        lock (_createLock)
        {
            if (_orderIdsByIdempotencyKey.TryGetValue(key, out var existingOrderId) &&
                _orders.TryGetValue(existingOrderId, out var existingOrder))
            {
                return new ShippingOrderCreationResult(existingOrder, false);
            }

            var order = new ShippingOrder(
                Guid.NewGuid(),
                request.PartnerId,
                orderNumber,
                request.DestinationName.Trim(),
                request.DestinationAddress.Trim(),
                request.ServiceLevel.Trim(),
                request.TotalWeightKg,
                DateTimeOffset.UtcNow);

            _orders[order.Id] = order;
            _orderIdsByIdempotencyKey[key] = order.Id;
            return new ShippingOrderCreationResult(order, true);
        }
    }

    private static string CreateIdempotencyKey(Guid partnerId, string orderNumber) =>
        $"{partnerId:N}:{orderNumber.ToUpperInvariant()}";
}
