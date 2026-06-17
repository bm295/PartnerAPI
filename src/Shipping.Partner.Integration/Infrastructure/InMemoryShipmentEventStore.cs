using System.Collections.Concurrent;
using Shipping.Partner.Integration.Application;
using Shipping.Partner.Integration.Domain;

namespace Shipping.Partner.Integration.Infrastructure;

public sealed class InMemoryShipmentEventStore : IShipmentEventStore
{
    private readonly ConcurrentQueue<ShipmentEventRecord> _events = new();

    public IReadOnlyCollection<ShipmentEventRecord> GetAll() => _events.ToArray();

    public IReadOnlyCollection<ShipmentEventRecord> GetByPartnerId(Guid partnerId) =>
        _events.Where(@event => @event.PartnerId == partnerId).ToArray();

    public ShipmentEventRecord Append(ShipmentEventRequest request)
    {
        var record = new ShipmentEventRecord(
            Guid.NewGuid(),
            request.PartnerId,
            request.TrackingNumber.Trim(),
            request.Status.Trim(),
            string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim(),
            request.OccurredAtUtc,
            DateTimeOffset.UtcNow);

        _events.Enqueue(record);
        return record;
    }
}
